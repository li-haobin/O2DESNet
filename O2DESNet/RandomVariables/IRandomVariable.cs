using System;

namespace O2DESNet.RandomVariables;

/// <summary>
/// Categorical Random Variables Interface
/// </summary>
public interface ICategoricalRandomVariable<T> : IRandomVariable<T> { }

/// <summary>
/// Continuous Random Variables Interface
/// </summary>
public interface IContinuousRandomVariable : IRandomVariable<double> { }

/// <summary>
/// Discrete Random Variables Interface
/// </summary>
public interface IDiscreteRandomVariable : IRandomVariable<int> { }

/// <summary>
/// Random Variables Interface
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRandomVariable<T>
{
    /// <summary>
    /// Gets or sets the mean value.
    /// </summary>
    double Mean { get; set; }

    /// <summary>
    /// Gets or sets the standard deviation value.
    /// </summary>
    double StandardDeviation { get; set; }

    /// <summary>
    /// Samples the specified random generator.
    /// </summary>
    /// <param name="rs">The random generator.</param>
    /// <returns>Sample value as <typeparam name="T"></returns>
    T Sample(Random rs);
}
