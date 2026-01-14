using System;

namespace O2DESNet.RandomVariables.Discrete;

public class Uniform : IDiscreteRandomVariable
{
    private int lowerBound = 0;
    private int upperBound = 1;
    private double mean = 0.5d;
    private double std = 0.5d;

    /// <summary>
    /// Gets or sets the lower bound.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Nothing between lower bound and upper bound if IncludeBound property is set to 'false'
    /// </exception>
    public int LowerBound
    {
        get { return lowerBound; }
        set
        {
            lowerBound = value;
            if (value > UpperBound)
            {
                UpperBound = value;
                mean = value;
                std = 0d;
            }
            else
            {
                double tempSquareSum = 0d;
                mean = (lowerBound + upperBound) / 2d;
                double n = upperBound - lowerBound + 1d;

                if (IncludeBound)
                {
                    for (int i = lowerBound; i <= upperBound; i++)
                        tempSquareSum += (i - mean) * (i - mean);
                    std = Math.Sqrt(tempSquareSum / n);
                }
                else
                {
                    if (upperBound - lowerBound <= 1)
                        throw new ArgumentOutOfRangeException("Nothing between lower bound and upper bound if IncludeBound property is set to 'false'");

                    for (int i = lowerBound + 1; i <= upperBound - 1; i++)
                        tempSquareSum += (i - mean) * (i - mean);
                    std = Math.Sqrt(tempSquareSum / n);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the upper bound.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Nothing between lower bound and upper bound if IncludeBound property is set to 'false'
    /// </exception>
    public int UpperBound
    {
        get { return upperBound; }
        set
        {
            upperBound = value;
            if (value < LowerBound)
            {
                LowerBound = value;
                mean = value;
                std = 0d;
            }
            else
            {
                mean = (lowerBound + upperBound) / 2d;
                double tempSquareSum = 0d;
                double n = upperBound - lowerBound + 1d;

                if (IncludeBound)
                {

                    for (int i = lowerBound; i <= upperBound; i++)
                        tempSquareSum += (i - mean) * (i - mean);
                    std = Math.Sqrt(tempSquareSum / n);
                }
                else
                {
                    if (upperBound - lowerBound <= 1)
                        throw new ArgumentOutOfRangeException("Nothing between lower bound and upper bound if IncludeBound property is set to 'false'");

                    for (int i = lowerBound + 1; i <= upperBound - 1; i++)
                        tempSquareSum += (i - mean) * (i - mean);
                    std = Math.Sqrt(tempSquareSum / n);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the mean.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Users not allowed to define discrete uniform random variable by setting mean value
    /// </exception>
    public double Mean
    {
        get => mean;
        set => throw new ArgumentException("Users not allowed to define discrete uniform random variable by setting mean value");
    }

    /// <summary>
    /// Gets or sets the standard deviation.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Users not allowed to define discrete uniform random variable by setting standard deviation value
    /// </exception>
    public double StandardDeviation
    {
        get => std;
        set => throw new ArgumentException("Users not allowed to define discrete uniform random variable by setting standard deviation value");
    }

    /// <summary>
    /// Gets or sets a value indicating whether [include bound].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [include bound]; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeBound { get; set; } = true;

    /// <summary>
    /// Samples the specified rs.
    /// </summary>
    /// <param name="rs">The rs.</param>
    /// <returns>Sample value</returns>
    public int Sample(Random rs)
    {
        int temp;
        if (IncludeBound)
        {
            int dummyLowerBound = LowerBound - 1;
            int dummyUpperBound = UpperBound + 1;
            temp = dummyLowerBound;
            while (temp == dummyLowerBound || temp == dummyUpperBound)
            {
                temp = Convert.ToInt32(Math.Round(dummyLowerBound + (dummyUpperBound - dummyLowerBound) * rs.NextDouble()));
            }
        }
        else
        {
            temp = LowerBound;
            while (temp == LowerBound || temp == UpperBound)
            {
                temp = Convert.ToInt32(Math.Round(LowerBound + (UpperBound - LowerBound) * rs.NextDouble()));
            }
        }

        return temp;
    }
}
