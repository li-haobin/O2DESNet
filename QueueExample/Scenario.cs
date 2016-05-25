using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueExample
{
    public class Scenario : O2DESNet.Scenario
    {
        public double HourlyArrivalRate { get; set; }
        public double HourlyServiceRate { get; set; }
        public TimeSpan GetInterArrivalTime(Random rs) { return TimeSpan.FromHours(Exponential.Sample(rs, HourlyArrivalRate)); }
        public TimeSpan GetServiceTime(Random rs) { return TimeSpan.FromHours(Exponential.Sample(rs, HourlyServiceRate)); }
    }
}
