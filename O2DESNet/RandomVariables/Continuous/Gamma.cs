using System;

namespace O2DESNet.RandomVariables.Continuous;

public class Gamma : IContinuousRandomVariable
{
    private double mean = 1;
    private double std = 1;
    private double cv = 1;
    private double alpha = 1;
    private double beta = 1;

    /// <summary>
    /// Gets or sets the mean value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// None positive mean value not applicable for beta distribution
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

            mean = value;
            cv = std / mean;
            alpha = mean * mean / std / std;
            beta = mean / std / std;
        }
    }

    /// <summary>
    /// Gets or sets the standard deviation value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
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
            if (value < 0)
                throw new ArgumentOutOfRangeException("A negative standard deviation not applicable");

            std = value;
            cv = std / mean;
            alpha = mean * mean / std / std;
            beta = mean / std / std;
        }
    }

    /// <summary>
    /// Coefficient of Variation [CV = σ/μ]
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A negative coefficient variation not applicable
    /// </exception>
    private double CV
    {
        get
        {
            return cv;
        }
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("A negative coefficient variation not applicable");

            cv = value;
            std = cv * mean;
            alpha = 1 / cv / cv;
            beta = mean / std / std;
        }
    }

    /// <summary>
    /// Gets or sets the alpha.
    /// Shape of Gamma distribution, refer to <see href="https://en.wikipedia.org/wiki/Gamma_distribution">
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A negative or zero alpha value not applicable</exception>
    public double Alpha
    {
        get
        {
            return alpha;
        }
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException("A negative or zero alpha value not applicable");

            alpha = value;
            mean = alpha / beta;
            std = Math.Sqrt(alpha / beta / beta);
            cv = std / mean;
        }
    }

    /// <summary>
    /// Gets or sets the beta.
    /// Rate of Gamma distribution, refer to <see href="https://en.wikipedia.org/wiki/Gamma_distribution">
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A negative or zero beta value not applicable
    /// </exception>
    public double Beta
    {
        get
        {
            return beta;
        }
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException("A negative or zero beta value not applicable");

            beta = value;
            mean = alpha / beta;
            std = Math.Sqrt(alpha / beta / beta);
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
        if (Mean == 0)
            return 0;
        if (CV == 0)
            return Mean;
        return MathNet.Numerics.Distributions.Gamma.Sample(rs, Alpha, Beta);
    }
}
