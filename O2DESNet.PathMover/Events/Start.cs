using System;

namespace O2DESNet.PathMover.Events
{
    internal class Start : Event<Scenario, Status>
    {
        protected override void Invoke()
        {
            foreach (var vehicle in Status.AllVehicles)
                Schedule(new Move { Vehicle = vehicle }, TimeSpan.FromHours(DefaultRS.NextDouble()));
        }
    }
}
