using System;

namespace O2DESNet.Distributions;

public static class Beta
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="mean"></param>
    /// <param name="cv">coefficient of variation</param>
    /// <returns></returns>
    public static double Sample(Random rs, double mean, double cv)
    {
        if (mean < 0)
            throw new Exception("Negative mean not applicable");
        if (cv < 0)
            throw new Exception("Negative coefficient of variation not applicable for beta distribution");
        if (mean == 0)
            return 0;
        if (cv == 0)
            return mean;
        var stddev = cv * mean;
        var a = mean * mean * (1 - mean) / stddev / stddev - mean;
        var b = (1 - mean) * (1 - mean) * mean / stddev / stddev + mean - 1;
        return MathNet.Numerics.Distributions.Beta.Sample(rs, a, b);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mean"></param>
    /// <param name="cv">coefficient of variation</param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static double CDF(double mean, double cv, double x)
    {
        if (mean <= 0)
            throw new Exception("Zero or negative mean not applicable");
        if (cv <= 0)
            throw new Exception("Zero or negative coefficient of variation not applicable for beta distribution");
        var sigma = cv * mean;
        var a = mean * mean * (1 - mean) / sigma / sigma - mean;
        var b = (1 - mean) * (1 - mean) * mean / sigma / sigma + mean - 1;
        return MathNet.Numerics.Distributions.Beta.CDF(a, b, x);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mean"></param>
    /// <param name="cv">coefficient of variation</param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static double InvCDF(double mean, double cv, double p)
    {
        if (mean <= 0)
            throw new Exception("Zero or negative mean not applicable");
        if (cv <= 0)
            throw new Exception("Zero or negative coefficient of variation not applicable for beta distribution");
        var sigma = cv * mean;
        var a = mean * mean * (1 - mean) / sigma / sigma - mean;
        var b = (1 - mean) * (1 - mean) * mean / sigma / sigma + mean - 1;
        return MathNet.Numerics.Distributions.Beta.InvCDF(a, b, p);
    }
}
