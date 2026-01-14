using Microsoft.Extensions.Logging;

using O2DESNet.Distributions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Standard;

/// <summary>
/// A nonhomogeneous arrival generator that produces Arrive events according to a time-varying rate.
/// 
/// Big picture
/// - The baseline input is <see cref="Statics.MeanHourlyRate"/>: the mean arrival rate (per hour) without seasonality.
/// - Seasonality is modeled multiplicatively across multiple calendars: hour-of-day (24), day-of-week (7), day-of-month (31),
///   month-of-year (12), year (arbitrary list), plus any number of customized cycles (interval + list of factors).
/// - Each factor list is normalized (see <see cref="Normalize"/>) so its average equals 1.0. Values represent relative intensity.
/// - The instantaneous rate at a given clock time is the product of the baseline rate and the active factors from all dimensions.
/// - For simulation efficiency, the process uses a thinning algorithm: it samples inter-arrival gaps from an exponential
///   distribution with parameter equal to a precomputed peak hourly rate (an upper bound), then accepts or rejects candidates
///   based on the ratio of current-factor to max-factor for each seasonal dimension.
/// 
/// The generator exposes a simple interface: call <see cref="Start"/> to begin generating arrivals and <see cref="End"/> to stop.
/// When an arrival is realized, <see cref="OnArrive"/> is invoked, and <see cref="Count"/> is incremented.
/// </summary>
public class PatternGenerator : Sandbox<PatternGenerator.Statics>, IGenerator
{
    /// <summary>
    /// Static configuration (assets) for <see cref="PatternGenerator"/>.
    /// Includes baseline mean rate and seasonal factor lists. All seasonality dimensions are optional.
    /// </summary>
    public class Statics : IAssets
    {
        /// <summary>
        /// Identifier of the configuration. Defaults to the type name.
        /// </summary>
        public string Id => GetType().Name;
        /// <summary>
        /// Baseline mean arrival rate (per hour). If no seasonal factors are provided, the inter-arrival time
        /// follows an exponential distribution with mean 1 / MeanHourlyRate.
        /// </summary>
        public double MeanHourlyRate { get; set; }
        /// <summary>
        /// 24 hourly seasonal factors. If shorter than 24, zeros are appended; if longer, the list is truncated.
        /// All zeros or null imply no hourly seasonality (treated as 1s after normalization).
        /// </summary>
        public List<double> SeasonalFactors_HoursOfDay { get; } = [];
        /// <summary>
        /// 7 daily factors for Sunday..Saturday. All zeros or null imply no weekly seasonality.
        /// </summary>
        public List<double> SeasonalFactors_DaysOfWeek { get; } = [];
        /// <summary>
        /// 31 factors for 1..31 day-of-month. All zeros or null imply no day-of-month seasonality.
        /// Internally adjusted by month length at runtime (see ScheduleToArrive day-of-month check).
        /// </summary>
        public List<double> SeasonalFactors_DaysOfMonth { get; } = [];
        /// <summary>
        /// 12 monthly factors for Jan..Dec. All zeros or null imply no monthly seasonality.
        /// </summary>
        public List<double> SeasonalFactors_MonthsOfYear { get; } = [];
        /// <summary>
        /// Yearly factors. Length is flexible and cycles with year index. All zeros or null imply no yearly seasonality.
        /// </summary>
        public List<double> SeasonalFactors_Years { get; } = [];

        /// <summary>
        /// Arbitrary custom seasonal cycles. Each item is a tuple of:
        /// - Interval: the base interval length for one factor step; the full cycle length is Interval * factors.Count
        /// - List of factors: repeats cyclically over time.
        /// If empty or null, no custom cycles are applied.
        /// </summary>
        public List<(TimeSpan, List<double>)> CustomizedSeasonalFactors { get; } = [];

        /// <summary>
        /// Factory method to create a sandbox instance with this static configuration.
        /// </summary>
        public PatternGenerator Sandbox(ILogger logger, int seed = 0) => new(logger, this, nameof(PatternGenerator), seed);
    }

    #region Dyanmic Properties
    /// <summary>
    /// The clock time when Start() was called, null before the generator is started.
    /// </summary>
    public TimeSpan? StartTime { get; private set; }
    /// <summary>
    /// Indicates whether the generator is actively producing arrivals.
    /// </summary>
    public bool IsOn { get; private set; }
    /// <summary>
    /// Total number of arrivals generated since the last warmup/reset.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Upper bound of instantaneous rate (per hour) used by thinning.
    /// Computed as MeanHourlyRate times the maxima of normalized seasonal factors across all configured dimensions.
    /// </summary>
    private double PeakHourlyRate { get; set; }

    // Pre-normalized, fixed-length seasonal factor lists (each with average 1.0 after Normalize)
    private List<double> Adjusted_SeasonalFactors_HoursOfDay { get; }
    private List<double> Adjusted_SeasonalFactors_DaysOfWeek { get; }
    private List<double> Adjusted_SeasonalFactors_DaysOfMonth { get; }
    private List<double> Adjusted_SeasonalFactors_MonthsOfYear { get; }
    private List<double> Adjusted_SeasonalFactors_Years { get; }
    private List<(TimeSpan Interval, List<double> SeasonalFactors)> Adjusted_CustomizedSeasonalFactors { get; }

    // Max values of each adjusted factor list, used to compute PeakHourlyRate and thinning acceptance ratios
    private double AdjMax_SeasonalFactor_HoursOfDay { get; set; }
    private double AdjMax_SeasonalFactor_DaysOfWeek { get; set; }
    private double AdjMax_SeasonalFactor_DaysOfMonth { get; set; }
    private double AdjMax_SeasonalFactor_MonthsOfYear { get; set; }
    private double AdjMax_SeasonalFactor_Years { get; set; }
    private List<double> AdjMax_CustomizedSeasonalFactors { get; }

    /// <summary>
    /// For custom seasonal cycles, tracks remaining time offset within the current cycle after jumping by an exponential inter-arrival sample.
    /// Maintains phase continuity when stepping through candidate arrival times.
    /// </summary>
    private List<TimeSpan> CustomizedSeasonalRemainders { get; }
    #endregion

    #region Events
    /// <summary>
    /// Start generating arrivals. Schedules the next candidate arrival according to thinning.
    /// </summary>
    public void Start()
    {
        if (!IsOn)
        {
            Logger?.LogInformation("Start");
            IsOn = true;
            StartTime = ClockTime;
            // Note: Do not reset Count here; unit tests expect cumulative count across Start/End cycles.
            ScheduleToArrive();
        }
    }

    /// <summary>
    /// Stop generating further arrivals. Pending scheduled candidates will be ignored when fired (due to IsOn check).
    /// </summary>
    public void End()
    {
        if (IsOn)
        {
            Logger?.LogInformation("End");
            IsOn = false;
        }
    }

    /// <summary>
    /// Core thinning loop. Repeatedly samples a candidate inter-arrival time from Exp(PeakHourlyRate)
    /// and accepts it with probability equal to the product of current-factor / max-factor across all dimensions.
    /// If rejected, a new candidate gap is sampled starting from the last candidate time.
    /// When accepted, schedules <see cref="Arrive"/> at the accepted time.
    /// </summary>
    private void ScheduleToArrive()
    {
        var time = ClockTime;
        while (true)
        {
            // Candidate inter-arrival gap drawn from exponential with parameter PeakHourlyRate (hours^-1)
            var hoursElapsed = Exponential.Sample(DefaultRS, 1 / PeakHourlyRate);

            // For each custom cycle, advance its phase by the elapsed time and figure out which factor index is active
            var customizedIndices = new List<int>();
            for (int i = 0; i < Adjusted_CustomizedSeasonalFactors.Count; i++)
            {
                var (interval, factors) = Adjusted_CustomizedSeasonalFactors[i];
                var sum = CustomizedSeasonalRemainders[i].TotalHours + hoursElapsed;
                // How many interval steps have elapsed within the cycle during this gap?
                var countIntervals = (int)Math.Floor(sum / interval.TotalHours);
                // Update remainder within the full cycle span (Interval * factorCount)
                CustomizedSeasonalRemainders[i] = TimeSpan.FromHours(sum % (interval.TotalHours * factors.Count));
                // Determine the active factor index after stepping
                customizedIndices.Add(countIntervals % factors.Count);
            }

            // Candidate arrival absolute time
            time = time + TimeSpan.FromHours(hoursElapsed);

            // Convert to a synthetic DateTime to conveniently access Hour/Day/Month fields.
            // Using DateTime.MinValue as an anchor as only the components are needed.
            var dt = DateTime.MinValue + time;

            // Thinning acceptance checks per dimension. Each check accepts with probability currentFactor / maxFactor.
            if (DefaultRS.NextDouble() > Adjusted_SeasonalFactors_HoursOfDay[dt.Hour] / AdjMax_SeasonalFactor_HoursOfDay)
                continue;
            if (DefaultRS.NextDouble() > Adjusted_SeasonalFactors_DaysOfWeek[(int)dt.DayOfWeek] / AdjMax_SeasonalFactor_DaysOfWeek)
                continue;

            // Day-of-month factor is adjusted by actual month length so that average over a month remains consistent.
            // Multiply by 31 and divide by days in the actual month to re-scale the normalized 31-day vector.
            if (DefaultRS.NextDouble() > Adjusted_SeasonalFactors_DaysOfMonth[dt.Day - 1] * 31 / DateTime.DaysInMonth(dt.Year, dt.Month) / AdjMax_SeasonalFactor_DaysOfMonth)
                continue;

            if (DefaultRS.NextDouble() > Adjusted_SeasonalFactors_MonthsOfYear[dt.Month - 1] / AdjMax_SeasonalFactor_MonthsOfYear)
                continue;

            // Year factors cycle by list length; dt.Year starts from 1 in DateTime.MinValue-based arithmetic
            if (DefaultRS.NextDouble() > Adjusted_SeasonalFactors_Years[(dt.Year - 1) % Adjusted_SeasonalFactors_Years.Count] / AdjMax_SeasonalFactor_Years)
                continue;

            #region For customized seasonality
            bool reject = false;
            for (int i = 0; i < Adjusted_CustomizedSeasonalFactors.Count; i++)
            {
                var idx = customizedIndices[i];
                var factors = Adjusted_CustomizedSeasonalFactors[i].SeasonalFactors;
                if (DefaultRS.NextDouble() > factors[idx] / AdjMax_CustomizedSeasonalFactors[i])
                {
                    reject = true;
                    break;
                }
            }

            if (reject)
                continue;
            #endregion

            // All checks passed: schedule the actual arrival
            Schedule(Arrive, time - ClockTime);
            break;
        }
    }

    /// <summary>
    /// Arrival event handler. Increments <see cref="Count"/>, schedules the next candidate arrival, and fires <see cref="OnArrive"/>.
    /// </summary>
    private void Arrive()
    {
        if (IsOn)
        {
            Logger?.LogInformation("Arrive");
            Logger?.LogDebug($"{ClockTime}:\t{this}\tArrive");

            Count++;
            // Continue the thinning loop to produce subsequent arrivals
            ScheduleToArrive();
            OnArrive.Invoke();
        }
    }

    /// <summary>
    /// Event raised whenever an arrival is accepted and processed.
    /// </summary>
    public event Action OnArrive = () => { };
    #endregion

    /// <summary>
    /// Convenience constructor without logger.
    /// </summary>
    public PatternGenerator(Statics assets, string id, int seed)
        : this(null, assets, id, seed) { }

    /// <summary>
    /// Constructs the generator and prepares normalized and max seasonal factors, as well as the peak hourly rate used by thinning.
    /// </summary>
    public PatternGenerator(ILogger? logger, Statics assets, string id, int seed)
        : base(logger, assets, id, seed)
    {
        IsOn = false;
        Count = 0;

        // Normalize supplied seasonal lists so that each has average 1.0 and proper length.
        Adjusted_SeasonalFactors_HoursOfDay = Normalize(Assets.SeasonalFactors_HoursOfDay, 24);
        Adjusted_SeasonalFactors_DaysOfWeek = Normalize(Assets.SeasonalFactors_DaysOfWeek, 7);
        Adjusted_SeasonalFactors_DaysOfMonth = Normalize(Assets.SeasonalFactors_DaysOfMonth, 31);
        Adjusted_SeasonalFactors_MonthsOfYear = Normalize(Assets.SeasonalFactors_MonthsOfYear, 12);
        Adjusted_SeasonalFactors_Years = Normalize(Assets.SeasonalFactors_Years);
        Adjusted_CustomizedSeasonalFactors = [];
        if (Assets.CustomizedSeasonalFactors != null)
            foreach (var (interval, factors) in Assets.CustomizedSeasonalFactors)
                Adjusted_CustomizedSeasonalFactors.Add((interval, Normalize(factors)));

        #region Set max factor and peak hourly rate
        // Capture maxima for acceptance probabilities and to compute the thinning upper bound (PeakHourlyRate)
        AdjMax_SeasonalFactor_HoursOfDay = Adjusted_SeasonalFactors_HoursOfDay.Max();
        AdjMax_SeasonalFactor_DaysOfWeek = Adjusted_SeasonalFactors_DaysOfWeek.Max();
        AdjMax_SeasonalFactor_DaysOfMonth = Adjusted_SeasonalFactors_DaysOfMonth.Max();
        AdjMax_SeasonalFactor_MonthsOfYear = Adjusted_SeasonalFactors_MonthsOfYear.Max();
        AdjMax_SeasonalFactor_Years = Adjusted_SeasonalFactors_Years.Max();
        AdjMax_CustomizedSeasonalFactors = Adjusted_CustomizedSeasonalFactors.Select(t => t.SeasonalFactors.Max()).ToList();

        // Peak hourly rate = baseline * product of maxima across all dimensions
        PeakHourlyRate = Assets.MeanHourlyRate;
        PeakHourlyRate *= AdjMax_SeasonalFactor_HoursOfDay;
        PeakHourlyRate *= AdjMax_SeasonalFactor_DaysOfWeek;
        PeakHourlyRate *= AdjMax_SeasonalFactor_DaysOfMonth;
        PeakHourlyRate *= AdjMax_SeasonalFactor_MonthsOfYear;
        PeakHourlyRate *= AdjMax_SeasonalFactor_Years;
        foreach (var max in AdjMax_CustomizedSeasonalFactors)
            PeakHourlyRate *= max;
        #endregion

        // Initialize custom cycle phase remainders to zero
        CustomizedSeasonalRemainders = Adjusted_CustomizedSeasonalFactors.Select(t => new TimeSpan()).ToList();
    }

    /// <summary>
    /// Normalize seasonal factor lists:
    /// - If null or sums to 0, returns a list of 1s (or [1] if length unspecified).
    /// - Negative values are clamped to 0.
    /// - If a target interval count is provided, the list is truncated/padded to that length (pad with 0s).
    /// - Finally, values are scaled so their average equals 1.0 (sum becomes count).
    /// </summary>
    private static List<double> Normalize(List<double> factors, int? nIntervals = null)
    {
        // return default if undefined
        if (factors == null || factors.Sum() == 0)
        {
            if (nIntervals != null)
                return Enumerable.Repeat(1d, nIntervals.Value).ToList();
            else
                return [1];
        }

        // remove the negative part, replace with 0
        factors = factors.Select(f => Math.Max(0, f)).ToList();

        // adjust the length
        if (nIntervals != null)
        {
            factors = factors.Take(nIntervals.Value).ToList();
            while (factors.Count < nIntervals.Value)
                factors.Add(0);
        }

        // standardize: scale so that average == 1.0
        var sum = factors.Sum();
        return factors.Select(f => f / sum * factors.Count).ToList();
    }

    /// <summary>
    /// Reset dynamic counters after warm-up.
    /// </summary>
    protected override void WarmedUpHandler()
    {
        Count = 0;
    }

    /// <summary>
    /// Clean up event subscriptions to avoid leaks and call base.Dispose to release child resources and counters.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();

        var handlers = OnArrive.GetInvocationList();
        foreach (Action i in handlers.Cast<Action>())
        {
            OnArrive -= i;
        }
    }
}
