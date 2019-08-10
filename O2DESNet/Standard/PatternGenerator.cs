using O2DESNet.Distributions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace O2DESNet.Standard
{
    public class PatternGenerator : Sandbox<PatternGenerator.Statics>, IGenerator
    {
        public class Statics : IAssets
        {
            public string Id { get { return GetType().Name; } }
            /// <summary>
            /// By default it follow exponential distribution
            /// </summary>
            public double MeanHourlyRate { get; set; }
            /// <summary>
            /// A list of 24 seasonal factors, to be filled with 0s if not full
            /// All 0s or null means no seasonal effect
            /// </summary>
            public List<double> SeasonalFactors_HoursOfDay { get; set; }
            /// <summary>
            /// A list of 7 seasonal factors, to be filled with 0s if not full
            /// All 0s or null means no seasonal effect
            /// </summary>
            public List<double> SeasonalFactors_DaysOfWeek { get; set; }
            /// <summary>
            /// A list of 31 seasonal factors, to be filled with 0s if not full
            /// All 0s or null means no seasonal effect
            /// </summary>
            public List<double> SeasonalFactors_DaysOfMonth { get; set; }
            /// <summary>
            /// A list of 12 seasonal factors, to be filled with 0s if not full
            /// All 0s or null means no seasonal effect
            /// </summary>
            public List<double> SeasonalFactors_MonthsOfYear { get; set; }
            /// <summary>
            /// All 0s or null means no seasonal effect
            /// </summary>
            public List<double> SeasonalFactors_Years { get; set; }
            public List<(TimeSpan, List<double>)> CustomizedSeasonalFactors { get; set; }
            public PatternGenerator Sandbox(int seed = 0) { return new PatternGenerator(this, seed); }
        }

        #region Dyanmic Properties
        public DateTime? StartTime { get; private set; }
        public bool IsOn { get; private set; }
        public int Count { get; private set; }
        private double PeakHourlyRate { get; set; }
        private List<double> Adjusted_SeasonalFactors_HoursOfDay { get; set; }
        private List<double> Adjusted_SeasonalFactors_DaysOfWeek { get; set; }
        private List<double> Adjusted_SeasonalFactors_DaysOfMonth { get; set; }
        private List<double> Adjusted_SeasonalFactors_MonthsOfYear { get; set; }
        private List<double> Adjusted_SeasonalFactors_Years { get; set; }
        private List<(TimeSpan Interval, List<double> SeasonalFactors)> Adjusted_CustomizedSeasonalFactors { get; set; }
        private double AdjMax_SeasonalFactor_HoursOfDay { get; set; }
        private double AdjMax_SeasonalFactor_DaysOfWeek { get; set; }
        private double AdjMax_SeasonalFactor_DaysOfMonth { get; set; }
        private double AdjMax_SeasonalFactor_MonthsOfYear { get; set; }
        private double AdjMax_SeasonalFactor_Years { get; set; }
        private List<double> AdjMax_CustomizedSeasonalFactors { get; set; }
        private List<TimeSpan> CustomizedSeasonalRemainders { get; set; }
        #endregion

        #region Events
        public void Start()
        {
            if (!IsOn)
            {
                Log("Start");
                IsOn = true;
                StartTime = ClockTime;
                Count = 0;
                ScheduleToArrive();
            }
        }

        public void End()
        {
            if (IsOn)
            {
                Log("End");
                IsOn = false;
            }
        }

        private void ScheduleToArrive()
        {
            var time = ClockTime;
            while (true)
            {
                var hoursElapsed = Exponential.Sample(DefaultRS, 1 / PeakHourlyRate);
                var customizedIndices = new List<int>();
                for (int i = 0; i < Adjusted_CustomizedSeasonalFactors.Count; i++)
                {
                    var (interval, factors) = Adjusted_CustomizedSeasonalFactors[i];
                    var sum = CustomizedSeasonalRemainders[i].TotalHours + hoursElapsed;
                    var countIntervals = (int)Math.Floor(sum / interval.TotalHours);
                    CustomizedSeasonalRemainders[i] = TimeSpan.FromHours(sum % (interval.TotalHours * factors.Count));
                    customizedIndices.Add(countIntervals % factors.Count);
                }
                time = time.AddHours(hoursElapsed);
                if (DefaultRS.NextDouble() > Adjusted_SeasonalFactors_HoursOfDay[time.Hour] / AdjMax_SeasonalFactor_HoursOfDay) continue;
                if (DefaultRS.NextDouble() > Adjusted_SeasonalFactors_DaysOfWeek[(int)time.DayOfWeek] / AdjMax_SeasonalFactor_DaysOfWeek) continue;
                if (DefaultRS.NextDouble() > Adjusted_SeasonalFactors_DaysOfMonth[time.Day - 1] * 31 / DateTime.DaysInMonth(time.Year, time.Month) / AdjMax_SeasonalFactor_DaysOfMonth) continue;
                if (DefaultRS.NextDouble() > Adjusted_SeasonalFactors_MonthsOfYear[time.Month - 1] / AdjMax_SeasonalFactor_MonthsOfYear) continue;
                if (DefaultRS.NextDouble() > Adjusted_SeasonalFactors_Years[(time.Year - 1) % Adjusted_SeasonalFactors_Years.Count] / AdjMax_SeasonalFactor_Years) continue;
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
                if (reject) continue;
                #endregion
                Schedule(Arrive, time);
                break;
            }
        }

        private void Arrive()
        {
            if (IsOn)
            {
                Log("Arrive");
                Debug.WriteLine("{0}:\t{1}\tArrive", ClockTime, this);

                Count++;
                ScheduleToArrive();
                OnArrive.Invoke();
            }
        }

        public event Action OnArrive = () => { };
        #endregion
        
        public PatternGenerator(Statics assets, int seed = 0, string tag = null)
            : base(assets, seed, tag)
        {
            IsOn = false;
            Count = 0;

            #region Normalize seasonal factors
            List<double> normalize(List<double> factors, int? nIntervals = null)
            {
                /// return default if undefined
                if (factors == null || factors.Sum() == 0)
                {
                    if (nIntervals != null) return Enumerable.Repeat(1d, nIntervals.Value).ToList();
                    else return new List<double> { 1 };
                }

                /// remove the negative part, replace with 0
                factors = factors.Select(f => Math.Max(0, f)).ToList();

                /// adjust the lenghth
                if (nIntervals != null)
                {
                    factors = factors.Take(nIntervals.Value).ToList();
                    while (factors.Count < nIntervals.Value) factors.Add(0);
                }

                /// standardize
                var sum = factors.Sum();
                return factors.Select(f => f / sum * factors.Count).ToList();
            }

            Adjusted_SeasonalFactors_HoursOfDay = normalize(Assets.SeasonalFactors_HoursOfDay, 24);
            Adjusted_SeasonalFactors_DaysOfWeek = normalize(Assets.SeasonalFactors_DaysOfWeek, 7);
            Adjusted_SeasonalFactors_DaysOfMonth = normalize(Assets.SeasonalFactors_DaysOfMonth, 31);
            Adjusted_SeasonalFactors_MonthsOfYear = normalize(Assets.SeasonalFactors_MonthsOfYear, 12);
            Adjusted_SeasonalFactors_Years = normalize(Assets.SeasonalFactors_Years);
            Adjusted_CustomizedSeasonalFactors = new List<(TimeSpan Interval, List<double> SeasonalFactors)>();
            if (Assets.CustomizedSeasonalFactors != null)
                foreach (var (interval, factors) in Assets.CustomizedSeasonalFactors)
                    Adjusted_CustomizedSeasonalFactors.Add((interval, normalize(factors)));
            #endregion

            #region Set max factor and peak hourly rate
            AdjMax_SeasonalFactor_HoursOfDay = Adjusted_SeasonalFactors_HoursOfDay.Max();
            AdjMax_SeasonalFactor_DaysOfWeek = Adjusted_SeasonalFactors_DaysOfWeek.Max();
            AdjMax_SeasonalFactor_DaysOfMonth = Adjusted_SeasonalFactors_DaysOfMonth.Max();
            AdjMax_SeasonalFactor_MonthsOfYear = Adjusted_SeasonalFactors_MonthsOfYear.Max();
            AdjMax_SeasonalFactor_Years = Adjusted_SeasonalFactors_Years.Max();
            AdjMax_CustomizedSeasonalFactors = Adjusted_CustomizedSeasonalFactors.Select(t => t.SeasonalFactors.Max()).ToList();
            PeakHourlyRate = Assets.MeanHourlyRate;
            PeakHourlyRate *= AdjMax_SeasonalFactor_HoursOfDay;
            PeakHourlyRate *= AdjMax_SeasonalFactor_DaysOfWeek;
            PeakHourlyRate *= AdjMax_SeasonalFactor_DaysOfMonth;
            PeakHourlyRate *= AdjMax_SeasonalFactor_MonthsOfYear;
            PeakHourlyRate *= AdjMax_SeasonalFactor_Years;
            foreach (var max in AdjMax_CustomizedSeasonalFactors) PeakHourlyRate *= max;
            #endregion

            CustomizedSeasonalRemainders = Adjusted_CustomizedSeasonalFactors.Select(t => new TimeSpan()).ToList();
        }

        protected override void WarmedUpHandler()
        {
            Count = 0;
        }

        public override void Dispose()
        {
            foreach (Action i in OnArrive.GetInvocationList()) OnArrive -= i;
        }

    }
}
