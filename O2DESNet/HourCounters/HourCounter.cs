using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.HourCounters;

public class HourCounter : IHourCounter, IDisposable
{
    // Private fields
    private readonly ILogger? _logger;
    private ISandbox _sandbox;
    private TimeSpan _initialTime;
    private Dictionary<TimeSpan, double> _history = [];

    // Backing fields for properties with private setters
    private TimeSpan _lastTime;
    private double _lastCount;
    private double _totalIncrement;
    private double _totalDecrement;
    private double _totalHours;
    private double _cumValue;
    private bool _paused;
    private bool _keepHistory;
    private ReadOnlyHourCounter? _readOnly;

    // Private properties
    private ReadOnlyHourCounter? ReadOnly => _readOnly;

    public ILogger? Logger => _logger;

    // Public properties and fields
    /// <summary>
    /// Last observed simulation time (from the sandbox clock) when the counter was most recently updated.
    /// Used as the left boundary of the next accumulation interval until another observation occurs.
    /// </summary>
    public TimeSpan LastTime => _lastTime;

    /// <summary>
    /// The most recent observed count value.
    /// Represents the level held since <see cref="LastTime"/> until the next observation is recorded.
    /// </summary>
    public double LastCount => _lastCount;

    /// <summary>
    /// Total amount of positive changes observed across the whole observation period.
    /// This increases when a new observation is larger than the previous <see cref="LastCount"/>
    /// </summary>
    public double TotalIncrement => _totalIncrement;

    /// <summary>
    /// Total amount of negative changes observed across the whole observation period.
    /// This increases when a new observation is smaller than the previous <see cref="LastCount"/>
    /// </summary>
    public double TotalDecrement => _totalDecrement;

    /// <summary>
    /// Total number of hours that have elapsed (and were accumulated) since the initial time.
    /// This value does not increase while the counter is <see cref="Paused"/>.
    /// </summary>
    public double TotalHours => _totalHours;

    /// <summary>
    /// Ratio of accumulated working time to wall-clock time since initialization.
    /// Computed as TotalHours divided by (LastTime - initialTime).TotalHours; returns 0 if no time has elapsed.
    /// </summary>
    public double WorkingTimeRatio
    {
        get
        {
            UpdateToClockTime();
            if (LastTime == _initialTime)
                return 0;
            return TotalHours / (LastTime - _initialTime).TotalHours;
        }
    }

    /// <summary>
    /// The cumulative time-weighted integral of the count (in hours * count).
    /// Each interval contributes duration (in hours) multiplied by the held <see cref="LastCount"/> during that interval.
    /// </summary>
    public double CumValue => _cumValue;

    /// <summary>
    /// The time-average of the count over the observation period.
    /// Equals <see cref="LastCount"/> if no hours have been accumulated yet (i.e., TotalHours == 0).
    /// </summary>
    public double AverageCount
    {
        get
        {
            UpdateToClockTime();
            if (TotalHours == 0)
                return LastCount;
            return CumValue / TotalHours;
        }
    }

    /// <summary>
    /// Average timespan that a load stays in the activity, assuming a stationary process
    /// (i.e., decrement rate == increment rate). Computed as AverageCount / DecrementRate (Little’s law).
    /// Returns 0 when undefined (e.g., no decrements observed yet).
    /// </summary>
    public TimeSpan AverageDuration
    {
        get
        {
            UpdateToClockTime();
            double hours = AverageCount / DecrementRate;
            if (double.IsNaN(hours) || double.IsInfinity(hours))
                hours = 0;
            return TimeSpan.FromHours(hours);
        }
    }

    /// <summary>
    /// Indicates whether the counter is currently paused. While paused, time is not accumulated and
    /// counts are not integrated into <see cref="CumValue"/>.
    /// </summary>
    public bool Paused => _paused;

    /// <summary>
    /// Indicates whether this counter keeps a timestamped history of observed counts.
    /// When enabled, <see cref="History"/> will expose the recorded scatter points.
    /// </summary>
    public bool KeepHistory => _keepHistory;

    /// <summary>
    /// Scatter points of (time since initialization, count) representing history entries.
    /// Prefer this strongly-typed struct over tuples for readability and maintainability.
    /// Returns null when <see cref="KeepHistory"/> is false to avoid unnecessary memory usage.
    /// </summary>
    public List<HourCounterHistoryPoint>? History
    {
        get
        {
            if (!_keepHistory)
                return null;
            return _history
                .OrderBy(i => i.Key)
                .Select(i => new HourCounterHistoryPoint(i.Key - _initialTime, i.Value))
                .ToList();
        }
    }

    /// <summary>
    /// Average increment rate per hour across the observation period.
    /// Calculated as TotalIncrement / TotalHours. Updates to the current clock time before evaluation.
    /// </summary>
    public double IncrementRate
    {
        get
        {
            UpdateToClockTime();
            return TotalIncrement / TotalHours;
        }
    }

    /// <summary>
    /// Average decrement rate per hour across the observation period.
    /// Calculated as TotalDecrement / TotalHours. Updates to the current clock time before evaluation.
    /// </summary>
    public double DecrementRate
    {
        get
        {
            UpdateToClockTime();
            return TotalDecrement / TotalHours;
        }
    }

    /// <summary>
    /// Distribution of total hours spent at each distinct count value.
    /// Key = count value, Value = total hours observed at that value.
    /// </summary>
    public Dictionary<double, double> HoursForCount = [];

    // Constructors
    internal HourCounter(ISandbox sandbox, bool keepHistory = false)
    {
        if(sandbox is null)
            throw new ArgumentNullException(nameof(sandbox));
        _sandbox = sandbox;
        _logger = null;
        Init(sandbox, TimeSpan.Zero, keepHistory);
    }

    internal HourCounter(ILogger? logger, ISandbox sandbox, bool keepHistory = false)
    {
        if (sandbox is null)
            throw new ArgumentNullException(nameof(sandbox));
        _sandbox = sandbox;
        _logger = logger;
        Init(sandbox, TimeSpan.Zero, keepHistory);
    }

    internal HourCounter(ISandbox sandbox, TimeSpan initialTime, bool keepHistory = false)
    {
        if (sandbox is null)
            throw new ArgumentNullException(nameof(sandbox));
        _sandbox = sandbox;
        _logger = null;
        Init(sandbox, initialTime, keepHistory);
    }

    internal HourCounter(ILogger? logger, ISandbox sandbox, TimeSpan initialTime, bool keepHistory = false)
    {
        if (sandbox is null)
            throw new ArgumentNullException(nameof(sandbox));
        _sandbox = sandbox;
        _logger = logger;
        Init(sandbox, initialTime, keepHistory);
    }

    private void Init(ISandbox sandbox, TimeSpan initialTime, bool keepHistory)
    {
        _sandbox = sandbox;
        _initialTime = initialTime;
        _lastTime = initialTime;
        _lastCount = 0;
        _totalIncrement = 0;
        _totalDecrement = 0;
        _totalHours = 0;
        _cumValue = 0;
        _keepHistory = keepHistory;
        if (_keepHistory)
            _history = [];
    }

    // Public methods
    /// <summary>
    /// Observe the specified count at the current sandbox clock time.
    /// Updates accumulated hours, time-weighted cumulative value, increment/decrement totals, and optional history logging.
    /// Throws if the current clock time is earlier than the previous observation time.
    /// </summary>
    /// <param name="count">The new observed count value.</param>
    public void ObserveCount(double count)
    {
        var clockTime = _sandbox.ClockTime;
        if (clockTime < LastTime)
            throw new Exception("Time of new count cannot be earlier than current time.");
        if (!Paused)
        {
            var hours = (clockTime - LastTime).TotalHours;
            _totalHours += hours;
            _cumValue += hours * LastCount;
            if (count > LastCount)
                _totalIncrement += count - LastCount;
            else
                _totalDecrement += LastCount - count;

            if (!HoursForCount.ContainsKey(LastCount))
                HoursForCount.Add(LastCount, 0);
            HoursForCount[LastCount] += hours;
        }

        // Log current point and, if changed, the new point
        _logger?.LogInformation("HourCounter sample: Hours={Hours}, Count={Count}, Paused={Paused}", TotalHours, LastCount, Paused);
        if (count != LastCount)
        {
            _logger?.LogInformation("HourCounter sample: Hours={Hours}, Count={Count}, Paused={Paused}", TotalHours, count, Paused);
        }

        _lastTime = clockTime;
        _lastCount = count;
        if (KeepHistory)
            _history[clockTime] = count;
    }

    /// <summary>
    /// Observe the specified count with a consistency check on the supplied clock time.
    /// The provided clockTime must match the sandbox's ClockTime. Kept for backward compatibility since v3.6 (Issue 1).
    /// </summary>
    /// <param name="count">The new observed count value.</param>
    /// <param name="clockTime">The time that must match the sandbox clock.</param>
    public void ObserveCount(double count, TimeSpan clockTime)
    {
        CheckClockTime(clockTime);
        ObserveCount(count);
    }

    /// <summary>
    /// Observe a change (delta) to the current count at the current sandbox clock time.
    /// Equivalent to calling ObserveCount(LastCount + change).
    /// </summary>
    /// <param name="change">The delta to apply to the last observed count.</param>
    public void ObserveChange(double change) => ObserveCount(LastCount + change);

    /// <summary>
    /// Observe a change (delta) with a consistency check on the supplied clock time.
    /// The provided clockTime must match the sandbox's ClockTime. Kept for backward compatibility since v3.6 (Issue 1).
    /// </summary>
    /// <param name="change">The delta to apply to the last observed count.</param>
    /// <param name="clockTime">The time that must match the sandbox clock.</param>
    public void ObserveChange(double change, TimeSpan clockTime)
    {
        CheckClockTime(clockTime);
        ObserveChange(change);
    }

    /// <summary>
    /// Pause the counter at the current sandbox clock time.
    /// Finalizes the current accumulation interval and prevents further accumulation until Resume is called.
    /// Also logs the paused state.
    /// </summary>
    public void Pause()
    {
        var clockTime = _sandbox.ClockTime;
        if (Paused)
            return;
        ObserveCount(LastCount, clockTime);
        _paused = true;
        _logger?.LogInformation("HourCounter paused: Hours={Hours}, Count={Count}", TotalHours, LastCount);
    }

    /// <summary>
    /// Pause the counter with a consistency check on the supplied clock time.
    /// The provided clockTime must match the sandbox's ClockTime. Kept for backward compatibility since v3.6 (Issue 1).
    /// </summary>
    /// <param name="clockTime">The time that must match the sandbox clock.</param>
    public void Pause(TimeSpan clockTime)
    {
        CheckClockTime(clockTime);
        Pause();
    }

    /// <summary>
    /// Resume the counter at the current sandbox clock time.
    /// Sets LastTime to the current clock time and enables accumulation for subsequent observations.
    /// Also logs the resume action.
    /// </summary>
    public void Resume()
    {
        if (!Paused)
            return;
        _lastTime = _sandbox.ClockTime;
        _paused = false;
        _logger?.LogInformation("HourCounter resumed: Hours={Hours}, Count={Count}", TotalHours, LastCount);
    }

    /// <summary>
    /// Resume the counter with a consistency check on the supplied clock time.
    /// The provided clockTime must match the sandbox's ClockTime. Kept for backward compatibility since v3.6 (Issue 1).
    /// </summary>
    /// <param name="clockTime">The time that must match the sandbox clock.</param>
    public void Resume(TimeSpan clockTime)
    {
        CheckClockTime(clockTime);
        Resume();
    }

    internal void WarmedUp()
    {

        // all reset except the last count
        _initialTime = _sandbox.ClockTime;
        _lastTime = _sandbox.ClockTime;
        _totalIncrement = 0;
        _totalDecrement = 0;
        _totalHours = 0;
        _cumValue = 0;
        HoursForCount = [];
    }

    /// <summary>
    /// Get the percentile of count values on time, i.e., the count value that with x-percent of time the observation is not higher than it.
    /// Uses the time spent at each distinct count value as weights.
    /// </summary>
    /// <param name="ratio">Percentile between 0 and 100 (e.g., 50 for median).</param>
    public double Percentile(double ratio)
    {
        SortHoursForCount();
        var threshold = HoursForCount.Sum(i => i.Value) * ratio / 100;
        foreach (var i in HoursForCount)
        {
            threshold -= i.Value;
            if (threshold <= 0)
                return i.Key;
        }

        return double.PositiveInfinity;
    }

    /// <summary>
    /// Build a histogram of the time spent within ranges of count values.
    /// Returns a dictionary mapping the lower bound of each interval to an array: { total hours observed, probability, cumulative probability }.
    /// </summary>
    /// <param name="countInterval">Width of the count value interval (bin size).</param>
    /// <returns>A dictionary map: lower bound -> [hours observed, probability, cumulative probability].</returns>
    public Dictionary<double, double[]> Histogram(double countInterval) // interval -> { observation, probability, cumulative probability}
    {
        SortHoursForCount();
        var histogram = new Dictionary<double, double[]>();
        if (HoursForCount.Count > 0)
        {
            double countLb = 0;
            double cumHours = 0;
            foreach (var i in HoursForCount)
            {
                if (i.Key > countLb + countInterval || i.Equals(HoursForCount.Last()))
                {
                    if (cumHours > 0)
                        histogram.Add(countLb, new double[] { cumHours, 0, 0 });
                    countLb += countInterval;
                    cumHours = i.Value;
                }
                else
                {
                    cumHours += i.Value;
                }
            }
        }

        var sum = histogram.Sum(h => h.Value[0]);
        double cum = 0;
        foreach (var h in histogram)
        {
            cum += h.Value[0];
            h.Value[1] = h.Value[0] / sum; // probability
            h.Value[2] = cum / sum; // cum. prob.
        }

        return histogram;
    }

    public ReadOnlyHourCounter AsReadOnly()
    {
        if (_readOnly == null)
            _readOnly = new ReadOnlyHourCounter(this);
        return _readOnly;
    }

    public void Dispose() { }

    // Private methods
    private void UpdateToClockTime()
    {
        if (LastTime != _sandbox.ClockTime)
            ObserveCount(LastCount);
    }


    private void CheckClockTime(TimeSpan clockTime)
    {
        if (clockTime != _sandbox.ClockTime)
            throw new Exception("ClockTime is not consistent with the Sandbox.");
    }

    private void SortHoursForCount() => HoursForCount = HoursForCount.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value);
}
