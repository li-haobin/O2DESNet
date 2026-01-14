using System;

namespace O2DESNet.Distributions;

public static class Exponential
{
    public static double Sample(Random rs, double mean)
    {
        return MathNet.Numerics.Distributions.Exponential.Sample(rs, 1 / mean);
    }

    public static double CDF(double mean, double x)
    {
        return MathNet.Numerics.Distributions.Exponential.CDF(1 / mean, x);
    }

    public static double InvCDF(double mean, double p)
    {
        return MathNet.Numerics.Distributions.Exponential.InvCDF(1 / mean, p);
    }

    public static TimeSpan Sample(Random rs, TimeSpan mean)
    {
        return TimeSpan.FromDays(Sample(rs, mean.TotalDays));
    }
}