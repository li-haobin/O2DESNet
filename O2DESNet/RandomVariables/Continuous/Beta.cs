using System;

namespace O2DESNet.RandomVariables.Continuous;

public class Beta : IContinuousRandomVariable
{
    private double mean = 0.5d;
    private double cv = Math.Sqrt(3d) / 3d;    // value: 0.57735026918962573105...
    private double std = Math.Sqrt(1d / 12d);  // value: 0.28867513459481286552...
    private double alpha = 1d;
    private double beta = 1d;

    /// <summary>
    /// Gets or sets the mean value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// None positive mean value not applicable for beta distribution
    /// or
    /// Mean value of beta distribution should not exceed 1 (one)
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The setting of mean and standard deviation will derive illegal alpha value
    /// </exception>
    public double Mean
    {
        get
        {
            return mean;
        }
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException("None positive mean value not applicable for beta distribution");

            if (value > 1)
                throw new ArgumentOutOfRangeException("Mean value of beta distribution should not exceed 1 (one)");

            mean = value;
            cv = std / mean;
            var alphaTemp = mean * mean * (1d - mean) / std / std - mean;
            var betaTemp = (1d - mean) * (1d - mean) * mean / std / std + mean - 1d;

            if (alphaTemp > 0d)
                alpha = alphaTemp;
            else
                throw new ArgumentException("The setting of mean and standard deviation will derive illegal alpha value");

            if (betaTemp > 0d)
                beta = betaTemp;
            else
                throw new ArgumentException("The setting of mean and standard deviation will derive illegal alpha value");
        }
    }

    /// <summary>
    /// Gets or sets the standard deviation value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Negative standard deviation not applicable
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The setting of mean and standard deviation will derive illegal alpha value
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
                throw new ArgumentOutOfRangeException("Negative standard deviation not applicable");

            std = value;
            cv = std / mean;

            var alphaTemp = mean * mean * (1d - mean) / std / std - mean;
            var betaTemp = (1 - mean) * (1d - mean) * mean / std / std + mean - 1d;

            if (alphaTemp > 0d)
                alpha = alphaTemp;
            else
                throw new ArgumentException("The setting of mean and standard deviation will derive illegal alpha value");

            if (betaTemp > 0d)
                beta = betaTemp;
            else
                throw new ArgumentException("The setting of mean and standard deviation will derive illegal alpha value");
        }
    }

    /// <summary>
    /// Gets or sets the Coefficient of Variation.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Negative coefficient variation not applicable
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
                throw new ArgumentOutOfRangeException("Negative coefficient variation not applicable");

            cv = value;
            std = cv * mean;
        }
    }

    /// <summary>
    /// Gets or sets the alpha value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A (-) negative or (0) zero alpha value not applicable
    /// </exception>
    public double AlphaValue
    {
        get
        {
            return alpha;
        }
        set
        {
            if (value <= 0d)
                throw new ArgumentOutOfRangeException("A negative or zero alpha value not applicable");

            alpha = value;
            mean = alpha / (alpha + beta);
            std = Math.Sqrt(alpha * beta / (alpha + beta) * (alpha + beta) / (alpha + beta + 1));
            cv = std / mean;
        }
    }

    /// <summary>
    /// Gets or sets the beta value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// negative or zero beta value not applicable
    /// </exception>
    public double BetaValue
    {
        get
        {
            return beta;
        }
        set
        {
            if (value <= 0d)
                throw new ArgumentOutOfRangeException("negative or zero beta value not applicable");

            beta = value;
            mean = alpha / (alpha + beta);
            std = Math.Sqrt(alpha * beta / (alpha + beta) * (alpha + beta) / (alpha + beta + 1));
            cv = std / mean;
        }
    }

    /// <summary>
    /// Samples the specified random generator.
    /// </summary>
    /// <param name="rs">The random generator.</param>
    /// <returns>Sample Value</returns>
    public double Sample(Random rs)
    {
        if (cv == 0d)
            return Mean;
        return MathNet.Numerics.Distributions.Beta.Sample(rs, AlphaValue, BetaValue);
    }
}
