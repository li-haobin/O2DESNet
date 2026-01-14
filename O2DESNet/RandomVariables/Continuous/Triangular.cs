using System;

namespace O2DESNet.RandomVariables.Continuous;

public class Triangular : IContinuousRandomVariable
{
    private double lowerBound = 0d;
    private double upperBound = 1d;
    private double mode = 0.5d;
    private double mean = 0.5d;
    private double std = Math.Sqrt(0.75d / 18d);  // value: 0.20412414523193150861501976578438...

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
            if (value > Mode)
                Mode = value;
            lowerBound = value;
            mean = (lowerBound + upperBound + mode) / 3d;
            std = Math.Sqrt(((lowerBound) * (lowerBound) +
                              (upperBound) * (upperBound) +
                              (mode) * (mode) -
                              (lowerBound) * (upperBound) -
                              (lowerBound) * (mode) -
                              (upperBound) * (mode)) / 18d);
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
            if (value < Mode)
                Mode = value;
            upperBound = value;
            mean = (lowerBound + upperBound + mode) / 3d;
            std = Math.Sqrt(((lowerBound) * (lowerBound) +
                              (upperBound) * (upperBound) +
                              (mode) * (mode) -
                              (lowerBound) * (upperBound) -
                              (lowerBound) * (mode) -
                              (upperBound) * (mode)) / 18d);
        }
    }

    /// <summary>
    /// Gets or sets the mode of the triangle distribution
    /// </summary>
    public double Mode
    {
        get
        {
            return mode;
        }
        set
        {
            if (value < LowerBound)
                LowerBound = value;
            if (value > UpperBound)
                UpperBound = value;
            mode = value;
            mean = (lowerBound + upperBound + mode) / 3d;
            std = Math.Sqrt(((lowerBound) * (lowerBound) +
                              (upperBound) * (upperBound) +
                              (mode) * (mode) -
                              (lowerBound) * (upperBound) -
                              (lowerBound) * (mode) -
                              (upperBound) * (mode)) / 18d);
        }
    }

    /// <summary>
    /// Gets or sets the mean value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Users not allowed to define triangular random variable by setting mean value
    /// </exception>
    public double Mean
    {
        get => mean;
        set => throw new ArgumentException("Users not allowed to define triangular random variable by setting mean value");
    }

    /// <summary>
    /// Gets or sets the standard deviation value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Users not allowed to define triangular random variable by setting standard deviation value
    /// </exception>
    public double StandardDeviation
    {
        get => std;
        set => throw new ArgumentException("Users not allowed to define triangular random variable by setting standard deviation value");
    }

    /// <summary>
    /// Samples the specified random generator.
    /// </summary>
    /// <param name="rs">The random generator.</param>
    /// <returns>Sample value</returns>
    public double Sample(Random rs)
    {
        return MathNet.Numerics.Distributions.Triangular.Sample(rs, LowerBound, UpperBound, Mode);
    }
}
