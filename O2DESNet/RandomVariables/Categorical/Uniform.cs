using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.RandomVariables.Categorical;

public class Uniform<T> : ICategoricalRandomVariable<T>
{
    private readonly string NotSupported
        = $"Categorical random variable (mean and standard deviation) for {nameof(Uniform<T>)} not available";

    /// <summary>
    /// Gets or sets the candidates.
    /// </summary>
    public IEnumerable<T>? Candidates { get; set; }

    /// <summary>
    /// Gets or sets the mean.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Categorical random variable standard deviation not available for <see cref="Uniform{T}" /> or/>
    /// Categorical random variable standard deviation not available for <see cref="Uniform{T}" />
    /// </exception>
    public double Mean
    {
        get => throw new NotImplementedException(NotSupported);
        set => throw new NotImplementedException(NotSupported);
    }

    /// <summary>
    /// Gets or sets the standard deviation.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Categorical random variable standard deviation not available for <see cref="Uniform{T}" /> or/>
    /// Categorical random variable standard deviation not available for <see cref="Uniform{T}" />
    /// </exception>
    public double StandardDeviation
    {
        get => throw new NotImplementedException(NotSupported);
        set => throw new NotImplementedException(NotSupported);
    }

    /// <summary>
    /// Samples the specified random generator.
    /// </summary>
    /// <param name="rs">The random generator.</param>
    /// <returns>Sample value as <typeparam name="T"></returns>
    public T? Sample(Random rs)
    {
        if (Candidates.Count() == 0)
            return default;
        return Candidates.ElementAt(rs.Next(Candidates.Count()));
    }
}
