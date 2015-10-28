using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Distribution
{
    public class Exponential
    {
        public static double Sample(Random rs, double mean)
        {
            return MathNet.Numerics.Distributions.Exponential.Sample(rs, 1.0 / mean);
        }
        public static TimeSpan Sample(Random rs, TimeSpan mean)
        {
            return TimeSpan.FromSeconds(Sample(rs, mean.TotalSeconds));
        }
    }
}
