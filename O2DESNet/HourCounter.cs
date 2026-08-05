using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace O2DESNet
{
    public interface IReadOnlyHourCounter
    {
        DateTime LastTime { get; }
        double LastCount { get; }
        bool Paused { get; }
        double TotalIncrement { get; }
        double TotalDecrement { get; }
        double IncrementRate { get; }
        double DecrementRate { get; }
        double TotalHours { get; }
        double WorkingTimeRatio { get; }
        double CumValue { get; }
        double AverageCount { get; }
        TimeSpan AverageDuration { get; }
        string? LogFile { get; set; }
    }

    public interface IHourCounter : IReadOnlyHourCounter
    {
        void ObserveCount(double count, DateTime clockTime);
        void ObserveChange(double change, DateTime clockTime);
        void Pause();
        void Pause(DateTime clockTime);
        void Resume(DateTime clockTime);
    }

    public class ReadOnlyHourCounter(HourCounter hourCounter) : IReadOnlyHourCounter, IDisposable
    {
        private readonly HourCounter HourCounter = hourCounter;

        public DateTime LastTime => HourCounter.LastTime;
        public double LastCount => HourCounter.LastCount;
        public bool Paused => HourCounter.Paused;
        public double TotalIncrement => HourCounter.TotalIncrement;
        public double TotalDecrement => HourCounter.TotalDecrement;
        public double IncrementRate => HourCounter.IncrementRate;
        public double DecrementRate => HourCounter.DecrementRate;
        public double TotalHours => HourCounter.TotalHours;
        public double WorkingTimeRatio => HourCounter.WorkingTimeRatio;
        public double CumValue => HourCounter.CumValue;
        public double AverageCount => HourCounter.AverageCount;
        public TimeSpan AverageDuration => HourCounter.AverageDuration;

        public string? LogFile
        {
            get => HourCounter.LogFile;
            set => HourCounter.LogFile = value;
        }

        public void Dispose() { }
    }

    public class HourCounter : IHourCounter, IDisposable
    {
        private ISandbox _sandbox;
        private DateTime _initialTime;
        public DateTime LastTime { get; private set; }
        public double LastCount { get; private set; }

        public double TotalIncrement { get; private set; }
        public double TotalDecrement { get; private set; }
        public double TotalHours { get; private set; }

        private void UpdateToClockTime()
        {
            if (LastTime != _sandbox.ClockTime) ObserveCount(LastCount);
        }

        public double WorkingTimeRatio
        {
            get
            {
                UpdateToClockTime();
                if (LastTime == _initialTime) return 0;
                return TotalHours / (LastTime - _initialTime).TotalHours;
            }
        }

        public double CumValue { get; private set; }

        public double AverageCount
        {
            get
            {
                UpdateToClockTime();
                if (TotalHours == 0) return LastCount;
                return CumValue / TotalHours;
            }
        }

        public TimeSpan AverageDuration
        {
            get
            {
                UpdateToClockTime();
                double hours = AverageCount / DecrementRate;
                if (double.IsNaN(hours) || double.IsInfinity(hours)) hours = 0;
                return TimeSpan.FromHours(hours);
            }
        }

        public bool Paused { get; private set; }

        #region For history keeping
        private Dictionary<DateTime, double>? _history;
        public bool KeepHistory { get; private set; }

        public List<Tuple<double, double>>? History
        {
            get
            {
                if (!KeepHistory) return null;
                return _history!
                    .OrderBy(i => i.Key)
                    .Select(i => Tuple.Create((i.Key - _initialTime).TotalHours, i.Value))
                    .ToList();
            }
        }
        #endregion

        internal HourCounter(ISandbox sandbox, bool keepHistory = false)
        {
            Init(sandbox, DateTime.MinValue, keepHistory);
        }

        internal HourCounter(ISandbox sandbox, DateTime initialTime, bool keepHistory = false)
        {
            Init(sandbox, initialTime, keepHistory);
        }

        private void Init(ISandbox sandbox, DateTime initialTime, bool keepHistory)
        {
            _sandbox = sandbox;
            _initialTime = initialTime;
            LastTime = initialTime;
            LastCount = 0;
            TotalIncrement = 0;
            TotalDecrement = 0;
            TotalHours = 0;
            CumValue = 0;
            KeepHistory = keepHistory;
            if (KeepHistory) _history = [];
        }

        public void ObserveCount(double count)
        {
            var clockTime = _sandbox.ClockTime;
            if (clockTime < LastTime)
                throw new Exception("Time of new count cannot be earlier than current time.");

            if (!Paused)
            {
                var hours = (clockTime - LastTime).TotalHours;
                TotalHours += hours;
                CumValue += hours * LastCount;
                if (count > LastCount) TotalIncrement += count - LastCount;
                else TotalDecrement += LastCount - count;

                if (!HoursForCount.ContainsKey(LastCount)) HoursForCount.Add(LastCount, 0);
                HoursForCount[LastCount] += hours;
            }

            if (_logFile != null)
            {
                using var sw = new StreamWriter(_logFile, append: true);
                sw.Write("{0},{1}", TotalHours, LastCount);
                if (Paused) sw.Write(",Paused");
                sw.WriteLine();
                if (count != LastCount)
                {
                    sw.Write("{0},{1}", TotalHours, count);
                    if (Paused) sw.Write(",Paused");
                    sw.WriteLine();
                }
            }

            LastTime = clockTime;
            LastCount = count;
            if (KeepHistory) _history![clockTime] = count;
        }

        public void ObserveCount(double count, DateTime clockTime)
        {
            CheckClockTime(clockTime);
            ObserveCount(count);
        }

        public void ObserveChange(double change) => ObserveCount(LastCount + change);

        public void ObserveChange(double change, DateTime clockTime)
        {
            CheckClockTime(clockTime);
            ObserveChange(change);
        }

        public void Pause()
        {
            var clockTime = _sandbox.ClockTime;
            if (Paused) return;
            ObserveCount(LastCount, clockTime);
            Paused = true;
            if (_logFile != null)
            {
                using var sw = new StreamWriter(_logFile, append: true);
                sw.WriteLine("{0},{1},Paused", TotalHours, LastCount);
            }
        }

        public void Pause(DateTime clockTime)
        {
            CheckClockTime(clockTime);
            Pause();
        }

        public void Resume()
        {
            if (!Paused) return;
            LastTime = _sandbox.ClockTime;
            Paused = false;
            if (_logFile != null)
            {
                using var sw = new StreamWriter(_logFile, append: true);
                sw.WriteLine("{0},{1},Paused", TotalHours, LastCount);
                sw.WriteLine("{0},{1}", TotalHours, LastCount);
            }
        }

        public void Resume(DateTime clockTime)
        {
            CheckClockTime(clockTime);
            Resume();
        }

        private void CheckClockTime(DateTime clockTime)
        {
            if (clockTime != _sandbox.ClockTime) throw new Exception("ClockTime is not consistent with the Sandbox.");
        }

        public double IncrementRate
        {
            get
            {
                UpdateToClockTime();
                return TotalIncrement / TotalHours;
            }
        }

        public double DecrementRate
        {
            get
            {
                UpdateToClockTime();
                return TotalDecrement / TotalHours;
            }
        }

        internal void WarmedUp()
        {
            _initialTime = _sandbox.ClockTime;
            LastTime = _sandbox.ClockTime;
            TotalIncrement = 0;
            TotalDecrement = 0;
            TotalHours = 0;
            CumValue = 0;
            HoursForCount = [];
        }

        public Dictionary<double, double> HoursForCount = [];

        private void SortHoursForCount()
        {
            HoursForCount = HoursForCount.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value);
        }

        public double Percentile(double ratio)
        {
            SortHoursForCount();
            var threshold = HoursForCount.Sum(i => i.Value) * ratio / 100;
            foreach (var i in HoursForCount)
            {
                threshold -= i.Value;
                if (threshold <= 0) return i.Key;
            }
            return double.PositiveInfinity;
        }

        public Dictionary<double, double[]> Histogram(double countInterval)
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
                        if (cumHours > 0) histogram.Add(countLb, [cumHours, 0, 0]);
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
                h.Value[1] = h.Value[0] / sum;
                h.Value[2] = cum / sum;
            }
            return histogram;
        }

        private string? _logFile;
        public string? LogFile
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
                    }
            }
        }

        private ReadOnlyHourCounter? ReadOnly { get; set; } = null;

        public ReadOnlyHourCounter AsReadOnly()
        {
            ReadOnly ??= new ReadOnlyHourCounter(this);
            return ReadOnly;
        }

        public void Dispose() { }
    }
}
