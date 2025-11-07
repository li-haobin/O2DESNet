using System;

namespace O2DESNet.RandomVariables.Continuous;

public class Uniform : IContinuousRandomVariable
{
    private double lowerBound = 0d;
    private double upperBound = 1d;
    private double mean = 0.5d;
    private double std = 0.5d;

    /// <summary>
    /// Gets or sets the lower bound.
    /// </summary>
    public double LowerBound
    {
        get
        {
            return lowerBound;
        }
        set
        {
            if (value > UpperBound)
                UpperBound = value;
            lowerBound = value;
            mean = (lowerBound + upperBound) / 2d;
            std = (upperBound - lowerBound) * (upperBound - lowerBound) / 12d;
        }
    }

    /// <summary>
    /// Gets or sets the upper bound.
    /// </summary>
    public double UpperBound
    {
        get
        {
            return upperBound;
        }
        set
        {
            if (value < LowerBound)
                LowerBound = value;
            upperBound = value;
            mean = (lowerBound + upperBound) / 2d;
            std = (upperBound - lowerBound) * (upperBound - lowerBound) / 12d;
        }
    }

    /// <summary>
    /// Gets or sets the mean value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Users not allowed to define continuous uniform random variable by setting mean value
    /// </exception>
    public double Mean
    {
        get => mean;
        set => throw new ArgumentException("Users not allowed to define continuous uniform random variable by setting mean value");
    }

    /// <summary>
    /// Gets or sets the standard deviation value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Users not allowed to define continuous uniform random variable by setting standard deviation value
    /// </exception>
    public double StandardDeviation
    {
        get => std;
        set => throw new ArgumentException("Users not allowed to define continuous uniform random variable by setting standard deviation value");
    }

    /// <summary>
    /// Samples the specified random generator.
    /// </summary>
    /// <param name="rs">The random generator.</param>
    /// <returns>Sample value</returns>
    public double Sample(Random rs)
    {
        return lowerBound + (upperBound - lowerBound) * rs.NextDouble();
    }
}
