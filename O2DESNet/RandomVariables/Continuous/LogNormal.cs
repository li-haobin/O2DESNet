using System;

namespace O2DESNet.RandomVariables.Continuous;

public class LogNormal : IContinuousRandomVariable
{
    private double mean = Math.Exp(0.5d);                       // value: 1.6487212707001281941643355821...
    private double std = Math.Sqrt(Math.E * Math.E - Math.E);   // value: 2.1611974158950877367146858887...
    private double cv = Math.Sqrt(Math.E - 1);                  // value: 1.3108324944320861593638483100...
    private double mu = 0d;
    private double sigma = 1d;

    /// <summary>
    /// Gets or sets the mean value.
    /// Expectation of LogNormal random variable.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// None positive mean value is not applicable for beta distribution
    /// </exception>
    public double Mean
    {
        get
        {
            return mean;
        }
        set
        {
            if (value <= 0d)
                throw new ArgumentOutOfRangeException("None positive mean value is not applicable for beta distribution");

            mean = value;
            mu = Math.Log(mean) - 0.5d * Math.Log(1d + std * std / mean / mean);
            sigma = Math.Sqrt(Math.Log(1d + std * std / mean / mean));

            if (value == 0d)
                cv = double.MaxValue;
            else
                cv = std / mean;
        }
    }

    /// <summary>
    /// Gets or sets the standard deviation value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A negative standard deviation is not applicable
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
                throw new ArgumentOutOfRangeException("A negative standard deviation is not applicable");

            std = value;
            mu = Math.Log(mean) - 0.5d * Math.Log(1d + std * std / mean / mean);
            sigma = Math.Sqrt(Math.Log(1d + std * std / mean / mean));

            if (mean != 0d)
                cv = std / mean;
        }
    }

    /// <summary>
    /// Gets or sets the Coefficient of Variation.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A negative coefficient of variation is not applicable for log normal distribution
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
                throw new ArgumentOutOfRangeException("A negative coefficient of variation is not applicable for log normal distribution");

            cv = value;
            std = cv * mean;
        }
    }

    /// <summary>
    /// The log-scale(mu) of the distribution
    /// </summary>
    public double Mu
    {
        get
        {
            return mu;
        }
        set
        {
            mu = value;
            mean = Math.Exp(mu + sigma * sigma / 2d);
            std = Math.Sqrt((Math.Exp(sigma * sigma) - 1) * Math.Exp(2d * mu + sigma * sigma));
            cv = std / mean;
        }
    }

    /// <summary>
    /// Gets or sets the sigma.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A negative shape parameter is not applicable
    /// </exception>
    public double Sigma
    {
        get
        {
            return sigma;
        }
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("A negative shape parameter is not applicable");

            sigma = value;
            mean = Math.Exp(mu + sigma * sigma / 2d);
            std = Math.Sqrt((Math.Exp(sigma * sigma) - 1d) * Math.Exp(2d * mu + sigma * sigma));
            cv = std / mean;
        }
    }

    /// <summary>
    /// Samples the specified random generator.
    /// </summary>
    /// <param name="rs">The random generator.</param>
    /// <returns>Sample value</returns>
    public double Sample(Random rs)
    {
        return MathNet.Numerics.Distributions.LogNormal.Sample(rs, Mu, Sigma);
    }
}
