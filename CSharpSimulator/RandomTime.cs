using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace CSharpSimulator
{
    public class RandomTime
    {
        public static TimeSpan Exponential(TimeSpan mean, Random rs = null)
        {
            if (rs == null) rs = new Random();
            return TimeSpan.FromMinutes(MathNet.Numerics.Distributions.Exponential.Sample(rs, 1.0 / mean.TotalMinutes));
        }
    }
}
