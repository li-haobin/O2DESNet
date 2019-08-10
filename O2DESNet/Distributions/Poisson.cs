using System;

namespace O2DESNet.Distributions
{
    public static class Poisson
    {
        public static int Sample(Random rs, double lambda)
        {
            return MathNet.Numerics.Distributions.Poisson.Sample(rs, lambda);
        }

        public static double CDF(double lambda, double x)
        {
            return MathNet.Numerics.Distributions.Poisson.CDF(lambda, x);
        }
    }
}
