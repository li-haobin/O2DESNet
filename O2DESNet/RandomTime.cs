using System;

namespace O2DESNet
{
    public class RandomTime
    {
        private static Random GetRS(Random rs)
        {
            if (rs == null) return new Random();
            return rs;
        }        
        public static TimeSpan Uniform(TimeSpan max, Random rs = null)
        {
            return TimeSpan.FromMinutes(max.TotalMinutes * GetRS(rs).NextDouble());
        }
        public static TimeSpan Uniform(TimeSpan min, TimeSpan max, Random rs = null)
        {
            return TimeSpan.FromMinutes((max.TotalMinutes - min.TotalMinutes) * GetRS(rs).NextDouble() + min.TotalMinutes);
        }
        public static TimeSpan Exponential(TimeSpan mean, Random rs = null)
        {
            return TimeSpan.FromMinutes(MathNet.Numerics.Distributions.Exponential.Sample(GetRS(rs), 1.0 / mean.TotalMinutes));
        }
    }
}
