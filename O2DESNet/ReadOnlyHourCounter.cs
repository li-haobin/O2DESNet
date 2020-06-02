using System;

namespace O2DESNet
{
    public class ReadOnlyHourCounter : IReadOnlyHourCounter, IDisposable
    {
        private readonly HourCounter _hourCounter;

        #region IReadOnlyHourCounter Public Members
        public DateTime LastTime => _hourCounter.LastTime;

        public double LastCount => _hourCounter.LastCount;

        public bool Paused => _hourCounter.Paused;

        public double TotalIncrement => _hourCounter.TotalIncrement;

        public double TotalDecrement => _hourCounter.TotalDecrement;

        public double IncrementRate => _hourCounter.IncrementRate;

        public double DecrementRate => _hourCounter.DecrementRate;

        public double TotalHours => _hourCounter.TotalHours;

        public double WorkingTimeRatio => _hourCounter.WorkingTimeRatio;

        public double CumValue => _hourCounter.CumValue;

        public double AverageCount => _hourCounter.AverageCount;

        public TimeSpan AverageDuration => _hourCounter.AverageDuration;

        public string LogFile
        {
            get => _hourCounter.LogFile;
            set => _hourCounter.LogFile = value;
        } 
        #endregion

        internal ReadOnlyHourCounter(HourCounter hourCounter)
        {
            _hourCounter = hourCounter;
        }

        public void Dispose() { }
    }
}