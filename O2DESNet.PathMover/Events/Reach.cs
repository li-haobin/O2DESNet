using O2DESNet.PathMover.Dynamics;
using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Events
{
    internal class Reach : Event<Scenario, Status>
    {
        public Vehicle Vehicle { get; set; }
        
        protected override void Invoke()
        {
            if (Vehicle.Target == null) return;
            if (Vehicle.Next != null) // in case vehicle is not moving, skip
            {
                if (!ClockTime.Equals(Vehicle.TimeToReach)) return; // for change of speed
                Vehicle.Reach(ClockTime);
                Status.Log("{0}\tReach: {1} {2} {3}", ClockTime.ToLongTimeString(), Vehicle, Vehicle.Current, Vehicle.Target);
            }
            if (Vehicle.Current.Equals(Vehicle.Target))
            {
                Vehicle.OnTarget();
                Vehicle.Target = null;
            }
            else Execute(new Move { Vehicle = Vehicle });
        }
    }
}
