using System.Numerics;

namespace O2DESNet.UnitTests.PmPathTests;

public readonly record struct ControlPoint
{
    public ControlPointId Id { get; init; }
    public string Name { get; }
    public Vector2 Start { get; init; }
    public Vector2 End { get; init; }

    public ControlPoint(ControlPointId id, string name, Vector2 start, Vector2 end)
    {
        (Id, Name, Start, End) = (id, name, start, end);
    }
}
