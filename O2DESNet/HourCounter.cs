using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace O2DESNet
{
    public class HourCounter : IHourCounter, IDisposable
    {
        #region Private Fields
        private Dictionary<double, double> _hoursForCount = new Dictionary<double, double>();

        private readonly Dictionary<DateTime, double> _history;
        private readonly ISandbox _sandbox;
        private readonly bool _keepHistory;

        private DateTime _initialTime;
        private DateTime _lastTime;
        private double _lastCount;
        private double _totalIncrement;
        private double _totalDecrement;
        private double _totalHours;
        private double _cumValue;
        private string _logFile;
        private bool _paused;
        private ReadOnlyHourCounter _readOnly;
        #endregion

        #region Public Properties
        public DateTime LastTime => _lastTime;

        public double LastCount => _lastCount;

        /// <summary>
        /// Total number of increment observed
        /// </summary>
        public double TotalIncrement => _totalIncrement;

        /// <summary>
        /// Total number of decrement observed
        /// </summary>
        public double TotalDecrement => _totalDecrement;

        /// <summary>
        /// Total number of hours since the initial time.
        /// </summary>
        public double TotalHours => _totalHours;

        private void UpdateToClockTime()
        {
            if (LastTime != _sandbox.ClockTime) ObserveCount(LastCount);
        }

        public double WorkingTimeRatio
        {
            get
            {
                UpdateToClockTime();
                if (_lastTime == _initialTime) return 0;
                return _totalHours / (_lastTime - _initialTime).TotalHours;
            }
        }

        /// <summary>
        /// The cumulative count value (integral) on time in unit of hours
        /// </summary>
        public double CumValue => _cumValue;

        /// <summary>
        /// The average count on observation period
        /// </summary>
        public double AverageCount
        {
            get
            {
                UpdateToClockTime();
                // if _totalHours = 0
                if (Math.Abs(_totalHours) < 1e-16) return LastCount;
                return CumValue / TotalHours;
            }
        }

        /// <summary>
        /// Average timespan that a load stays in the activity, if it is a stationary process, 
        /// i.e., decrement rate == increment rate
        /// It is 0 at the initial status, i.e., decrement rate is NaN (no decrement observed).
        /// </summary>
        public TimeSpan AverageDuration
        {
            get
            {
                UpdateToClockTime();
                var hours = AverageCount / DecrementRate;
                if (double.IsNaN(hours) || double.IsInfinity(hours)) hours = 0;
                return TimeSpan.FromHours(hours);
            }
        }

        public bool Paused => _paused;

        #region For history keeping

        /// <summary>
        /// Gets a value indicating whether to keep history.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [keep history]; otherwise, <c>false</c>.
        /// </value>
        public bool KeepHistory => _keepHistory;

        /// <summary>
        /// Scatter points of (time in hours, count)
        /// </summary>
        public List<Tuple<double, double>> History
        {
            get
            {
                if (!_keepHistory) return null;
                return _history?.OrderBy(i => i.Key)
                    .Select(i => new Tuple<double, double>((i.Key - _initialTime).TotalHours, i.Value)).ToList();
            }
        }

        #endregion

        public double IncrementRate
        {
            get
            {
                UpdateToClockTime();
                return _totalIncrement / _totalHours;
            }
        }

        public double DecrementRate
        {
            get
            {
                UpdateToClockTime();
                return _totalDecrement / _totalHours;
            }
        }

        /// <summary>
        /// Statistics for the amount of time spent at each range of count values
        /// </summary>
        /// <param name="countInterval">width of the count value interval</param>
        /// <returns>A dictionary map from [the lower bound value of each interval] to the array of [total hours observed], [probability], [cumulated probability]</returns>
        public Dictionary<double, double[]> Histogram(double countInterval) // interval -> { observation, probability, cumulative probability}
        {
            SortHoursForCount();
            var histogram = new Dictionary<double, double[]>();
            if (_hoursForCount.Count > 0)
            {
                double countLb = 0;
                double cumHours = 0;
                foreach (var i in _hoursForCount)
                {
                    if (i.Key > countLb + countInterval || i.Equals(_hoursForCount.Last()))
                    {
                        if (cumHours > 0) histogram.Add(countLb, new double[] { cumHours, 0, 0 });
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

        public string LogFile
        {
            get => _logFile;
            set
            {
                _logFile = value;
                if (_logFile != null)
                    using (var sw = new StreamWriter(_logFile))
                    {
                        sw.WriteLine("Hours,Count,Remark");
                        sw.WriteLine("{0},{1}", TotalHours, LastCount);
                    };
            }
        }
        #endregion

        internal HourCounter(ISandbox sandbox, DateTime initialTime, bool keepHistory)
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
            if (_keepHistory) _history = new Dictionary<DateTime, double>();
        }

        internal HourCounter(ISandbox sandbox, bool keepHistory) : this(sandbox, DateTime.MinValue, keepHistory) { }

        #region Public Methods
        /// <summary>
        /// Observes the count.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <exception cref="Exception">Time of new count cannot be earlier than current time.</exception>
        public void ObserveCount(double count)
        {
            var clockTime = _sandbox.ClockTime;

            if (clockTime < LastTime)
                throw new Exception("Time of new count cannot be earlier than current time.");

            if (!Paused)
            {
                var hours = (clockTime - LastTime).TotalHours;
                _totalHours += hours;
                _cumValue += hours * _lastCount;
                if (count > LastCount) _totalIncrement += count - _lastCount;
                else _totalDecrement += LastCount - count;

                if (!_hoursForCount.ContainsKey(LastCount)) _hoursForCount.Add(LastCount, 0);
                _hoursForCount[LastCount] += hours;
            }

            if (_logFile != null)
            {
                using (var sw = new StreamWriter(_logFile, append: true))
                {
                    sw.Write("{0},{1}", TotalHours, LastCount);
                    if (Paused) sw.Write(",Paused");
                    sw.WriteLine();

                    if (Math.Abs(count - LastCount) > 1e-16)
                    {
                        sw.Write("{0},{1}", TotalHours, count);
                        if (Paused) sw.Write(",Paused");
                        sw.WriteLine();
                    }
                };
            }

            _lastTime = clockTime;
            _lastCount = count;

            if (_keepHistory) _history[clockTime] = count;
        }

        /// <summary>
        /// Remove parameter clockTime as since Version 3.6, according to Issue 1
        /// </summary>
        public void ObserveCount(double count, DateTime clockTime)
        {
            CheckClockTime(clockTime);
            ObserveCount(count);
        }

        /// <summary>
        /// Observes the change.
        /// </summary>
        /// <param name="change">The change.</param>
        public void ObserveChange(double change) => ObserveCount(LastCount + change);

        /// <summary>
        /// Remove parameter clockTime as since Version 3.6, according to Issue 1
        /// </summary>
        public void ObserveChange(double change, DateTime clockTime)
        {
            CheckClockTime(clockTime);
            ObserveChange(change);
        }

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        public void Pause()
        {
            var clockTime = _sandbox.ClockTime;
            if (Paused) return;
            ObserveCount(LastCount, clockTime);
            _paused = true;

            if (_logFile == null) return;

            using (var sw = new StreamWriter(_logFile, append: true))
            {
                sw.WriteLine("{0},{1},Paused", TotalHours, LastCount);
            };
        }

        /// <summary>
        /// Remove parameter clockTime as since Version 3.6, according to Issue 1
        /// </summary>
        public void Pause(DateTime clockTime)
        {
            CheckClockTime(clockTime);
            Pause();
        }

        /// <summary>
        /// Resumes this instance.
        /// </summary>
        public void Resume()
        {
            if (!Paused) return;
            _lastTime = _sandbox.ClockTime;
            _paused = false;

            if (_logFile == null) return;

            using (var sw = new StreamWriter(_logFile, append: true))
            {
                sw.WriteLine("{0},{1},Paused", TotalHours, LastCount);
                sw.WriteLine("{0},{1}", TotalHours, LastCount);
            };
        }

        /// <summary>
        /// Remove parameter clockTime as since Version 3.6, according to Issue 1
        /// </summary>
        public void Resume(DateTime clockTime)
        {
            CheckClockTime(clockTime);
            Resume();
        }

        /// <summary>
        /// Get this Hour Counter instance as a read only Hour Counter.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyHourCounter AsReadOnly()
        {
            if (_readOnly != null) return _readOnly;
            return (_readOnly = new ReadOnlyHourCounter(this));
        }

        /// <summary>
        /// Get the percentile of count values on time, i.e., the count value that with x-percent of time the observation is not higher than it.
        /// </summary>
        /// <param name="ratio">values between 0 and 100</param>
        public double Percentile(double ratio)
        {
            SortHoursForCount();

            var threshold = _hoursForCount.Sum(i => i.Value) * ratio / 100;

            foreach (var i in _hoursForCount)
            {
                threshold -= i.Value;
                if (threshold <= 0) return i.Key;
            }

            return double.PositiveInfinity;
        }

        public void Dispose() { }
        #endregion

        internal void WarmedUp()
        {
            // all reset except the last count
            _initialTime = _sandbox.ClockTime;
            _lastTime = _sandbox.ClockTime;
            _totalIncrement = 0;
            _totalDecrement = 0;
            _totalHours = 0;
            _cumValue = 0;
            _hoursForCount = new Dictionary<double, double>();
        }

        private void CheckClockTime(DateTime clockTime)
        {
            if (clockTime != _sandbox.ClockTime) throw new Exception("ClockTime is not consistent with the Sandbox.");
        }

        private void SortHoursForCount()
        {
            _hoursForCount = _hoursForCount.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value);
        }
    }
}
