using System;

namespace O2DESNet
{
    public class HourCounter
    {
        private O2DES _o2des;
        private DateTime _initialTime;
        public DateTime CurrentTime;
        public int CurrentCount { get; private set; }
        /// <summary>
        /// Total number of increment observed
        /// </summary>
        public int TotalIncrementCount { get; private set; }
        /// <summary>
        /// Total number of decrement observed
        /// </summary>
        public int TotalDecrementCount { get; private set; }
        /// <summary>
        /// Total number of hours since the initial time.
        /// </summary>
        public double TotalHours { get { return (CurrentTime - _initialTime).TotalHours; } }
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
        public HourCounter(O2DES o2des)
        {
            Init(o2des.ClockTime);
            _o2des = o2des;
        }
        private void Init(DateTime initialTime)
        {
            _o2des = null;
            _initialTime = initialTime;
            CurrentTime = initialTime;
            CurrentCount = 0;
            TotalIncrementCount = 0;
            TotalDecrementCount = 0;
            CumValue = 0;
        }
        public void ObserveCount(int count) { ObserveCount(count, _o2des.ClockTime); }
        public void ObserveCount(int count, DateTime timestamp)
        {
            if (timestamp < CurrentTime)
                throw new Exception("Time of new count cannot be earlier than current time.");
            if (count > CurrentCount) TotalIncrementCount += count - CurrentCount;
            else TotalDecrementCount += CurrentCount - count;
            CumValue += (timestamp - CurrentTime).TotalHours * CurrentCount;
            CurrentTime = timestamp;
            CurrentCount = count;
        }
        public void ObserveChange(int change) { ObserveChange(change, _o2des.ClockTime); }
        public void ObserveChange(int change, DateTime timestamp) { ObserveCount(CurrentCount + change, timestamp); }
    }
}
