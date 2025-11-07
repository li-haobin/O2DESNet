using System;

namespace O2DESNet.RandomVariables.Discrete;

public class Poisson : IDiscreteRandomVariable
{
    private double _lambda = 1d;
    private double _mean = 1d;
    private double _std = 1d;

    /// <summary>
    /// Gets or sets the lambda (Arrival Rate).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A negative lambda (arrival rate) is not applicable
    /// </exception>
    public double Lambda
    {
        get { return _lambda; }
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("A negative lambda (arrival rate) is not applicable");

            _lambda = value;
            _mean = value;
            _std = Math.Sqrt(value);
        }
    }

    /// <summary>
    /// Gets or sets the mean.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A negative mean value is not applicable
    /// </exception>
    public double Mean
    {
        get { return _mean; }
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("A negative mean value is not applicable");

            _mean = value;
            _lambda = value;
            _std = Math.Sqrt(value);
        }
    }

    /// <summary>
    /// Gets or sets the standard deviation.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A negative standard deviation is not applicable
    /// </exception>
    public double StandardDeviation
    {
        get { return _std; }
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("A negative standard deviation is not applicable");

            _std = value;
            _mean = value * value;
            _lambda = value * value;
        }
    }

    /// <summary>
    /// Samples the specified rs.
    /// </summary>
    /// <param name="rs">The rs.</param>
    /// <returns>Sample value</returns>
    public int Sample(Random rs)
    {
        return MathNet.Numerics.Distributions.Poisson.Sample(rs, Lambda);
    }
}
