using System;

namespace O2DESNet.Distributions;

public static class Gamma
{
    public static double Sample(Random rs, double mean, double cv)
    {
        if (mean == 0)
            return 0;
        if (cv == 0)
            return mean;
        var k = 1 / cv / cv;
        var lambda = k / mean;
        return MathNet.Numerics.Distributions.Gamma.Sample(rs, k, lambda);
    }

    public static double CDF(double mean, double cv, double x)
    {
        if (cv == 0)
            return x >= mean ? 1 : 0;
        var k = 1 / cv / cv;
        var lambda = k / mean;
        return MathNet.Numerics.Distributions.Gamma.CDF(k, lambda, x);
    }

    public static double InvCDF(double mean, double cv, double p)
    {
        if (cv == 0)
            return mean;
        var k = 1 / cv / cv;
        var lambda = k / mean;
        return MathNet.Numerics.Distributions.Gamma.InvCDF(k, lambda, p);
    }

    public static TimeSpan Sample(Random rs, TimeSpan mean, double cv)
    {
        return TimeSpan.FromDays(Sample(rs, mean.TotalDays, cv));
    }
}