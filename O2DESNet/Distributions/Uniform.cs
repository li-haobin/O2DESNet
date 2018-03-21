using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Distributions
{
    public static class Uniform
    {
        public static double Sample(Random rs, double lowerbound, double upperbound)
        {
            return lowerbound + (upperbound - lowerbound) * rs.NextDouble();
        }

        public static TimeSpan Sample(Random rs, TimeSpan lowerbound, TimeSpan upperbound)
        {
            return TimeSpan.FromMinutes(Sample(rs, lowerbound.TotalMinutes, upperbound.TotalMinutes));
        }

        public static T Sample<T>(Random rs, IEnumerable<T> candidates)
        {
            if (candidates.Count() == 0) return default(T);
            return candidates.ElementAt(rs.Next(candidates.Count()));
        }
    }
}