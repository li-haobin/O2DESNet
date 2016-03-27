using MathNet.Numerics.Distributions;
using System;

namespace O2DESNet.Demos.MM1Queue
{
    public class Scenario : O2DESNet.Scenario
    {
        public TimeSpan ExpectedInterArrivalTime { get; private set; }
        public TimeSpan ExpectedServiceTime { get; private set; }
        public Scenario(TimeSpan expectedInterArrivalTime, TimeSpan expectedServiceTime)
        {
            ExpectedInterArrivalTime = expectedInterArrivalTime;
            ExpectedServiceTime = expectedServiceTime;
        }

        internal TimeSpan InterArrivalTime(Random rs)
        {
            return TimeSpan.FromHours(Exponential.Sample(rs, 1.0 / ExpectedInterArrivalTime.TotalHours));
        }
        internal TimeSpan ServiceTime(Random rs)
        {
            //return TimeSpan.FromHours(Exponential.Sample(rs, 1.0 / ExpectedServiceTime.TotalHours));
            var mean = ExpectedServiceTime.TotalHours;
            var offset = mean;
            return TimeSpan.FromHours(mean - offset + rs.NextDouble() * offset * 2);
        }
    }
}
