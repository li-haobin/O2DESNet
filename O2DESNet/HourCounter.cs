using System;

namespace O2DESNet
{
    public class HourCounter
    {
        private O2DES _o2des;
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
        public HourCounter(DateTime? intialTime = null)
        {
            if (intialTime == null) Init(DateTime.MinValue);
            else Init(intialTime.Value);
        }
        public HourCounter(O2DES o2des, DateTime? initialTime = null)
        {
            if (initialTime == null) Init(o2des.ClockTime);
            else Init(initialTime.Value);
            _o2des = o2des;
        }
        private void Init(DateTime initialTime)
        {
            _o2des = null;
            _initialTime = initialTime;
            LastTime = initialTime;
            LastCount = 0;
            TotalIncrementCount = 0;
            TotalDecrementCount = 0;
            CumValue = 0;
        }
        public void ObserveCount(double count) { ObserveCount(count, _o2des.ClockTime); }
        public void ObserveCount(double count, DateTime timestamp)
        {
            if (timestamp < LastTime)
                throw new Exception("Time of new count cannot be earlier than current time.");
            if (count > LastCount) TotalIncrementCount += count - LastCount;
            else TotalDecrementCount += LastCount - count;
            CumValue += (timestamp - LastTime).TotalHours * LastCount;
            LastTime = timestamp;
            LastCount = count;
        }
        public void ObserveChange(double change) { ObserveChange(change, _o2des.ClockTime); }
        public void ObserveChange(double change, DateTime timestamp) { ObserveCount(LastCount + change, timestamp); }
    }
}
