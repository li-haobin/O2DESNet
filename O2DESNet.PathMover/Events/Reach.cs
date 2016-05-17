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
        public Vehicle Vehicle { get; private set; }
        public ControlPoint Target { get; private set; }
        public Action OnTarget { get; private set; }

        public Reach(Vehicle vehicle, ControlPoint target, Action onTarget)
        {
            Vehicle = vehicle;
            Target = target;
            OnTarget = onTarget;
        }
        protected override void Invoke()
        {
            if (!ClockTime.Equals(Vehicle.TimeToReach)) return; // for change of speed
            Vehicle.Reach(ClockTime);
            Status.Log("{0}\tReach:{1},{2}", ClockTime.ToLongTimeString(), Vehicle.Current, Target);
            if (Vehicle.Current.Equals(Target)) OnTarget();
            else Execute(new Move(Vehicle, Target, OnTarget));            
        }
    }
}
