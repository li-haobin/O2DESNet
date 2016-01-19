using System;
using System.Linq;

namespace O2DESNet.Distribution
{
    public class Empirical
    {
        public static int Sample(Random rs, double[] ratios)
        {
            var v = ratios.Sum() * rs.NextDouble();
            double sum = 0;
            for (int i = 0; i < ratios.Length - 1; i++)
            {
                sum += ratios[i];
                if (v < sum) return i;
            }
            return ratios.Length - 1;
        }
    }
}
