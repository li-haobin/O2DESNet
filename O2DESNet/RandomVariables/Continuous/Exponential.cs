using System;

namespace O2DESNet.RandomVariables.Continuous;

public class Exponential : IContinuousRandomVariable
{
    private double lambda = 1d;
    private double mean = 1d;
    private double std = 1d;

    /// <summary>
    /// Gets or sets the lambda.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// negative or zero arrival rate not applicable
    /// </exception>
    public double Lambda
    {
        get
        {
            return lambda;
        }
        set
        {
            if (value <= 0d)
                throw new ArgumentOutOfRangeException("negative or zero arrival rate not applicable");

            lambda = value;
            mean = 1d / lambda;
            std = mean;
        }
    }

    /// <summary>
    /// Gets or sets the mean value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// negative or zero mean value not applicable
    /// </exception>
    public double Mean
    {
        get { return mean; }
        set
        {
            if (value <= 0d)
                throw new ArgumentOutOfRangeException("negative or zero mean value not applicable");

            mean = value;
            std = mean;
            lambda = 1d / mean;
        }
    }

    /// <summary>
    /// Gets or sets the standard deviation value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// negative or zero standard deviation not applicable
    /// </exception>
    public double StandardDeviation
    {
        get { return std; }
        set
        {
            if (value <= 0d)
                throw new ArgumentOutOfRangeException("negative or zero standard deviation not applicable");

            std = value;
            mean = std;
            lambda = 1d / mean;
        }
    }

    /// <summary>
    /// Samples the specified random generator.
    /// </summary>
    /// <param name="rs">The random generator.</param>
    /// <returns>Sample value</returns>
    public double Sample(Random rs)
    {
        return MathNet.Numerics.Distributions.Exponential.Sample(rs, Lambda);
    }
}
