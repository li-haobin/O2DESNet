using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Distributions;

public static class Empirical
{
    public static int Sample(Random rs, IEnumerable<double> ratios)
    {
        var threshold = rs.NextDouble() * ratios.Sum();
        for (int i = 0; i < ratios.Count(); i++)
        {
            var v = ratios.ElementAt(i);
            if (threshold < v)
                return i;
            threshold -= v;
        }

        return -1;
    }
    public static T Sample<T>(Random rs, Dictionary<T, double> ratios)
    {
        return ratios.Keys.ElementAt(Sample(rs, ratios.Values));
    }
}
