using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Examples.MM1Queue
{
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
