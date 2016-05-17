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
        
        protected override void Invoke()
        {
            if (!Vehicle.Current.Equals(Vehicle.Target))
            {
                Vehicle.Move(Vehicle.Current.RoutingTable[Vehicle.Target], ClockTime);
                foreach(var v in Status.VehiclesOnPath[Vehicle.Current.PathingTable[Vehicle.Next]] )
                Schedule(new Reach { Vehicle = v }, v.TimeToReach.Value);
            }
            else Execute(new Reach { Vehicle = Vehicle });
            Status.Log("{0}\tMove: {1} {2} {3} {4}", ClockTime.ToLongTimeString(), Vehicle, Vehicle.Current, Vehicle.Next, Vehicle.Target);

            Status.Display_VehiclesOnPath();
            Console.ReadKey();
        }
    }
}
