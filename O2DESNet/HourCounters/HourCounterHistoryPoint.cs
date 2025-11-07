using System;

namespace O2DESNet.HourCounters;

/// <summary>
/// Represents a single history point for HourCounter, capturing the time since initialization and the observed count.
/// Immutable value type for efficient storage and clear semantics compared to tuples.
/// </summary>
public readonly struct HourCounterHistoryPoint
{
    public TimeSpan HoursSinceInitial { get; }
    public double Count { get; }

    public HourCounterHistoryPoint(TimeSpan hoursSinceInitial, double count)
    {
        HoursSinceInitial = hoursSinceInitial;
        Count = count;
    }
}
