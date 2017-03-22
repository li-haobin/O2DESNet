using System;
using System.Collections.Generic;
using System.Linq;

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
        public double TotalHours { get; private set; }
        public double WorkingTimeRatio
        {
            get
            {
                if (LastTime == _initialTime) return 0;
                return TotalHours / (LastTime - _initialTime).TotalHours;
            }
        }
        /// <summary>
        /// The cumulative count value on time in unit of hours
        /// </summary>
        public double CumValue { get; private set; }
        /// <summary>
        /// The average count on observation period
        /// </summary>
        public double AverageCount { get { return CumValue / TotalHours; } }
        public bool Paused { get; private set; }

        public HourCounter() { Init(DateTime.MinValue); }
        public HourCounter(DateTime initialTime) { Init(initialTime); }
        private void Init(DateTime initialTime)
        {
            _initialTime = initialTime;
            LastTime = initialTime;
            LastCount = 0;
            TotalIncrementCount = 0;
            TotalDecrementCount = 0;
            TotalHours = 0;
            CumValue = 0;
        }
        public void ObserveCount(double count, DateTime clockTime)
        {
            if (Paused) return;
            //if (timestamp < LastTime)
            //    throw new Exception("Time of new count cannot be earlier than current time.");
            if (count > LastCount) TotalIncrementCount += count - LastCount;
            else TotalDecrementCount += LastCount - count;
            if (clockTime > LastTime)
            {
                var hours = (clockTime - LastTime).TotalHours;
                TotalHours += hours;
                CumValue += hours * LastCount;
                LastTime = clockTime;

                if (!HoursForCount.ContainsKey(LastCount)) HoursForCount.Add(LastCount, 0);
                HoursForCount[LastCount] += hours;
            }
            LastCount = count;
        }
        public void ObserveChange(double change, DateTime clockTime) { ObserveCount(LastCount + change, clockTime); }
        public void Pause() { Pause(LastTime); }
        public void Pause(DateTime clockTime)
        {
            if (Paused) return;
            ObserveChange(0, clockTime);
            Paused = true;
        }
        public void Resume(DateTime clockTime)
        {
            if (!Paused) return;
            LastTime = clockTime;
            Paused = false;
        }

        public double IncrementRate { get { return TotalIncrementCount / TotalHours; } }
        public double DecrementRate { get { return TotalDecrementCount / TotalHours; } }
        public void WarmedUp(DateTime clockTime)
        {
            // all reset except the last count
            _initialTime = clockTime;
            LastTime = clockTime;
            TotalIncrementCount = 0;
            TotalDecrementCount = 0;
            TotalHours = 0;
            CumValue = 0;
            HoursForCount = new Dictionary<double, double>();
        }

        public Dictionary<double, double> HoursForCount = new Dictionary<double, double>();
        private void SortHoursForCount() { HoursForCount = HoursForCount.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value); }
        /// <summary>
        /// Get the percentile of count values on time, i.e., the count value that with x-percent of time the observation is not higher than it.
        /// </summary>
        /// <param name="ratio">values between 0 and 100</param>
        public double Percentile(double ratio)
        {
            SortHoursForCount();
            var threashold = HoursForCount.Sum(i => i.Value) * ratio / 100;
            foreach (var i in HoursForCount)
            {
                threashold -= i.Value;
                if (threashold <= 0) return i.Key;
            }
            return double.PositiveInfinity;
        }
        /// <summary>
        /// Statistics for the amount of time spent at each range of count values
        /// </summary>
        /// <param name="countInterval">width of the count value interval</param>
        /// <returns>A dictionary map from [the lowerbound value of each interval] to the array of [total hours observed], [probability], [cumulated probability]</returns>
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
    }
}
