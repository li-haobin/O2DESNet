using System;

namespace O2DESNet
{
    public class HourCounter
    {
        private DateTime _initialTime;
        public DateTime LastTime;
        public double LastCount { get; private set; }
        /// <summary>
        /// Total number of increment observed
        /// </summary>
        public double TotalIncrementCount { get; private set; }
        /// <summary>
        /// Total number of decrement observed
        /// </summary>
        public double TotalDecrementCount { get; private set; }
        /// <summary>
        /// Total number of hours since the initial time.
        /// </summary>
        public double TotalHours { get { return (LastTime - _initialTime).TotalHours; } }
        /// <summary>
        /// The cumulative count value on time in unit of hours
        /// </summary>
        public double CumValue { get; private set; }
        /// <summary>
        /// The average count on observation period
        /// </summary>
        public double AverageCount { get { return CumValue / TotalHours; } }
        public HourCounter(DateTime initialTime)
        {
            _initialTime = initialTime;
            LastTime = initialTime;
            LastCount = 0;
            TotalIncrementCount = 0;
            TotalDecrementCount = 0;
            CumValue = 0;
        }
        public void ObserveCount(double count, DateTime timestamp)
        {
            //if (timestamp < LastTime)
            //    throw new Exception("Time of new count cannot be earlier than current time.");
            if (count > LastCount) TotalIncrementCount += count - LastCount;
            else TotalDecrementCount += LastCount - count;
            if (timestamp > LastTime)
            {
                CumValue += (timestamp - LastTime).TotalHours * LastCount;
                LastTime = timestamp;
            }
            LastCount = count;
        }
        public void ObserveChange(double change, DateTime timestamp) { ObserveCount(LastCount + change, timestamp); }
    }
}
