using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace Test
{
    /// <summary>
    /// The Scenario class that specifies what to simulate
    /// </summary>
    public class Scenario : O2DESNet.Scenario
    {
        public double HourlyArrivalRate { get; private set; }
        public double HourlyServiceRate { get; private set; }
        public TimeSpan GetInterArrivalTime(Random rs) { return TimeSpan.FromHours(Exponential.Sample(rs, HourlyArrivalRate)); }
        public TimeSpan GetServiceTime(Random rs) { return TimeSpan.FromHours(Exponential.Sample(rs, HourlyServiceRate)); }

        public Scenario(double hourlyArrivalRate, double hourlyServiceRate)
        {
            HourlyArrivalRate = hourlyArrivalRate;
            HourlyServiceRate = hourlyServiceRate;
        }
    }
}
