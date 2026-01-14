using System;
using System.Numerics;

namespace O2DESNet.UnitTests.PmPathTests;

public readonly record struct PmPath
{
    /// <summary>
    /// The maximum allowed number of vehicles on the path segment
    /// </summary>
    public int Capacity { get; }
    public ControlPoint Start { get; }
    public ControlPoint End { get; }
    /// <summary>
    /// Number of lanes per path segment. Each lane has the same capacity.
    /// </summary>
    /// <example>
    /// A two-lane path with a capacity of 5 vehicles per lane would have a total effective capacity of 10 vehicles.
    /// </example>
    public int NumberOfLanes { get; } = 1; // Default is always 1 lane
    public int EffectiveCapacity { get; }
    public double Length { get; }

    public PmPath(int capacity, ControlPoint start, ControlPoint end, int numberOfLanes)
    {
        if (numberOfLanes < 1)
            throw new ArgumentOutOfRangeException(nameof(numberOfLanes), "Number of lanes must be at least 1");

        Capacity = capacity;
        Start = start;
        End = end;
        NumberOfLanes = numberOfLanes;
        EffectiveCapacity = capacity * numberOfLanes;
        Length = PathLength();
    }

    private double PathLength() => Vector2.Distance(Start.End, End.Start);
}
