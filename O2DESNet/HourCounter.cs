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
        public HourCounter() { Init(DateTime.MinValue); }
        public HourCounter(DateTime initialTime) { Init(initialTime); }
        private void Init(DateTime initialTime)
        {
            _initialTime = initialTime;
            LastTime = initialTime;
            LastCount = 0;
            TotalIncrementCount = 0;
            TotalDecrementCount = 0;
            CumValue = 0;
        }
        public void ObserveCount(double count, DateTime clockTime)
        {
            //if (timestamp < LastTime)
            //    throw new Exception("Time of new count cannot be earlier than current time.");
            if (count > LastCount) TotalIncrementCount += count - LastCount;
            else TotalDecrementCount += LastCount - count;
            if (clockTime > LastTime)
            {
                CumValue += (clockTime - LastTime).TotalHours * LastCount;
                LastTime = clockTime;

            }
            LastCount = count;
        }
        public void ObserveChange(double change, DateTime clockTime) { ObserveCount(LastCount + change, clockTime); }
        public double IncrementRate { get { return TotalIncrementCount / TotalHours; } }
        public double DecrementRate { get { return TotalDecrementCount / TotalHours; } }
        public void WarmedUp(DateTime clockTime)
        {
            // all reset except the last count
            _initialTime = clockTime;
            LastTime = clockTime;
            TotalIncrementCount = 0;
            TotalDecrementCount = 0;
            CumValue = 0;
        }
    }
}
