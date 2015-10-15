using O2DESNet.PathMover.Dynamics;
using System;

namespace O2DESNet.PathMover.Exceptions
{
    class InfeasibleConstruction : Exception
    {
        public InfeasibleConstruction() { }
        public InfeasibleConstruction(string message) : base(message) { }
        public InfeasibleConstruction(string message, Exception inner) : base(message, inner) { }
    }
    class InfeasibleTravelling : Exception
    {
        public InfeasibleTravelling() { }
        public InfeasibleTravelling(string message) : base(message) { }
        public InfeasibleTravelling(string message, Exception inner) : base(message, inner) { }
    }

    class VechicleStatusError : Exception
    {
        public Vehicle Vehicle { get; private set; }
        public VechicleStatusError(Vehicle vehicle, string message) : base(message) { Vehicle = vehicle; }
        public VechicleStatusError(Vehicle vehicle, string message, Exception inner) : base(message, inner) { }
    }

}
