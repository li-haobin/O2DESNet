using O2DESNet.Standard;

using System;

namespace O2DESNet.UnitTests.PmPathTests;

public sealed class VehicleId : IEquatable<VehicleId>
{
    // Private Fields
    private readonly Guid _value;

    // Public Properties
    public Guid Value => _value;

    // Constructors
    public VehicleId()
    {
        _value = Guid.NewGuid();
    }

    public VehicleId(Guid value)
    {
        _value = value;
    }

    public static VehicleId New() => new(Guid.NewGuid());

    // Public Methods
    public override string ToString() => _value.ToString("n");

    public bool Equals(VehicleId? other)
    {
        if (other is null)
        {
            return false;
        }

        return _value.Equals(other._value);
    }

    public override bool Equals(object? obj) => obj is VehicleId other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(VehicleId? left, VehicleId? right) => Equals(left, right);

    public static bool operator !=(VehicleId? left, VehicleId? right) => !Equals(left, right);
}

public class Vehicle : Entity<VehicleId>
{
    private readonly string _name;

    public string Name => _name;

    public Vehicle(VehicleId id, string name) : base(id)
    {
        _name = name;
    }
}
