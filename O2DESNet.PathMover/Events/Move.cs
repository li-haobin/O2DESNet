using O2DESNet.PathMover.Dynamics;
using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Events
{
    internal class Move : Event<Scenario, Status>
    {
        public Vehicle Vehicle { get; set; }
        public ControlPoint Target { get; set; }
        public Action OnTarget { get; set; }

        public Move(Vehicle vehicle, ControlPoint target, Action onTarget)
        {
            Vehicle = vehicle;
            Target = target;
            OnTarget = onTarget;
        }
        protected override void Invoke()
        {
            Vehicle.Move(Vehicle.Current.RoutingTable[Target], ClockTime);
            Schedule(new Reach(Vehicle, Target, OnTarget), Vehicle.TimeToReach.Value);
            Status.Log("{0}\tMove:{1},{2},{3}", ClockTime.ToLongTimeString(), Vehicle.Current, Vehicle.Next, Target);
        }
    }
}
