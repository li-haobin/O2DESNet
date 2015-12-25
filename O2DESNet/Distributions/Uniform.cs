using System;

namespace O2DESNet.Distribution
{
    public class Uniform
    {
        public static TimeSpan Sample(Random rs, TimeSpan upperbound)
        {
            return TimeSpan.FromSeconds(rs.NextDouble() * upperbound.TotalSeconds);
        }
    }
}
