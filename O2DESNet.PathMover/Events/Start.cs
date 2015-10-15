using O2DESNet.PathMover.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Events
{
    internal class Start : Event
    {
        internal Start(Simulator sim) : base(sim) { }
        public override void Invoke()
        {
            foreach (var vehicle in _sim.Status.AllVehicles)
                _sim.ScheduleEvent(new Move(_sim, vehicle), TimeSpan.FromHours(_sim.RS.NextDouble()));
        }
    }
}
