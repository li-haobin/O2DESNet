using System;

namespace O2DESNet.RandomVariables.Continuous;

public class Normal : IContinuousRandomVariable
{
    private double mean = 1d;
    private double std = 1d;
    private double cv = 1d;

    /// <summary>
    /// Gets or sets the mean value.
    /// </summary>
    public double Mean
    {
        get
        {
            return mean;
        }
        set
        {
            mean = value;

            if (value == 0d)
                cv = double.MaxValue;
            else
                cv = std / Math.Abs(mean);
        }
    }
    /// <summary>
    /// standard deviation
    /// </summary>
    /// <exception cref="Exception">
    /// A negative standard deviation not applicable
    /// </exception>
    public double StandardDeviation
    {
        get
        {
            return std;
        }
        set
        {
            if (value < 0d)
                throw new Exception("A negative standard deviation is not applicable");

            std = value;
            if (mean != 0d)
                cv = std / Math.Abs(mean);
        }
    }

    /// <summary>
    /// Gets or sets the Coefficient of Variation.
    /// </summary>
    /// <exception cref="Exception">
    /// A negative coefficient of variation is not applicable
    /// </exception>
    public double CV
    {
        get
        {
            return cv;
        }
        set
        {
            if (value < 0d)
                throw new Exception("A negative coefficient of variation is not applicable");

            cv = value;
            std = cv * Math.Abs(mean);
        }
    }

    /// <summary>
    /// Samples the specified random generator.
    /// </summary>
    /// <param name="rs">The random generator.</param>
    /// <returns>Sample value</returns>
    public double Sample(Random rs)
    {
        if (cv == 0d)
            return mean;
        return MathNet.Numerics.Distributions.Normal.Sample(rs, mean, std);
    }
}
