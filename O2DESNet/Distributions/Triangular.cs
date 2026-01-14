using System;

namespace O2DESNet.Distributions;

public static class Triangular
{
    /// <summary>
    /// parameters definition
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static double Sample(Random rs, double lower, double upper, double mode)
    {
        return MathNet.Numerics.Distributions.Triangular.Sample(rs, lower, upper, mode);
    }
    public static double CDF(double lower, double upper, double mode, double x)
    {
        return MathNet.Numerics.Distributions.Triangular.CDF(lower, upper, mode, x);
    }
    public static double InvCDF(double lower, double upper, double mode, double p)
    {
        return MathNet.Numerics.Distributions.Triangular.InvCDF(lower, upper, mode, p);
    }
}
