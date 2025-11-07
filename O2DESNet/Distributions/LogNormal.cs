using System;

namespace O2DESNet.Distributions;

public static class LogNormal
{
    /// <summary>
    /// parameters definition
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
            throw new Exception("Negative coefficient of variation not applicable for log normal distribution");
        if (mean == 0)
            return 0;
        if (cv == 0)
            return mean;
        var stddev = cv * mean;
        return MathNet.Numerics.Distributions.LogNormal.Sample(rs, mean, stddev);
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
        if (cv == 0)
            return x >= mean ? 1 : 0;
        if (mean <= 0)
            throw new Exception("Zero or negative mean not applicable");
        var stddev = cv * mean;
        return MathNet.Numerics.Distributions.LogNormal.CDF(mean, stddev, x);
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
        if (cv == 0)
            return mean;
        if (mean <= 0)
            throw new Exception("Zero or negative mean not applicable");
        var stddev = cv * mean;
        return MathNet.Numerics.Distributions.LogNormal.InvCDF(mean, stddev, p);
    }
}
