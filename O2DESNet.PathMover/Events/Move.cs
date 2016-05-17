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
            if (!Target.Equals(Vehicle.Current))
            {
                Vehicle.Move(Vehicle.Current.RoutingTable[Target], ClockTime);
                Schedule(new Reach(Vehicle, Target, OnTarget), Vehicle.TimeToReach.Value);
            }
            else Execute(new Reach(Vehicle, Target, OnTarget));
            Status.Log("{0}\tMove: {1} {2} {3} {4}", ClockTime.ToLongTimeString(), Vehicle, Vehicle.Current, Vehicle.Next, Target);
        }
    }
}
