using MathNet.Numerics.Distributions;
using System;

namespace O2DESNet.Demos.MM1Queue
{
    public class Scenario
    {
        public TimeSpan ExpectedInterArrivalTime { get; private set; }
        public TimeSpan ExpectedServiceTime { get; private set; }
        public Scenario(TimeSpan expectedInterArrivalTime, TimeSpan expectedServiceTime)
        {
            ExpectedInterArrivalTime = expectedInterArrivalTime;
            ExpectedServiceTime = expectedServiceTime;
        }

        internal TimeSpan Generate_InterArrivalTime(Random rs)
        {
            return TimeSpan.FromHours(Exponential.Sample(rs, 1.0 / ExpectedInterArrivalTime.TotalHours));
        }
        internal TimeSpan Generate_ServiceTime(Random rs)
        {
            return TimeSpan.FromHours(Exponential.Sample(rs, 1.0 / ExpectedServiceTime.TotalHours));
        }
    }
}
