using O2DESNet.Standard;

using System;

namespace O2DESNet.UnitTests.PmPathTests;

public abstract class VehicleEventArgs : EventArgs
{
    public IEntity Vehicle { get; }
    public PmPath? Path { get; }

    protected VehicleEventArgs(IEntity vehicle, PmPath? pmPath)
    {
        Vehicle = vehicle;
        Path = pmPath;
    }
}

public class VehicleArrivedEventArgs(IEntity vehicle, PmPath? pmPath) : VehicleEventArgs(vehicle, pmPath);
public class VehicleProcessedEventArgs(IEntity vehicle, PmPath? pmPath) : VehicleEventArgs(vehicle, pmPath);
public class VehicleDepartedEventArgs(IEntity vehicle, PmPath? pmPath) : VehicleEventArgs(vehicle, pmPath);
