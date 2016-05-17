using O2DESNet.PathMover.Dynamics;
using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Events
{
    public class Move : Event<Scenario, Status>
    {
        public Vehicle Vehicle { get; set; }

        protected override void Invoke()
        {
            if (!Vehicle.Current.Equals(Vehicle.Targets.First()))
            {
                Vehicle.Move(Vehicle.Current.RoutingTable[Vehicle.Targets.First()], ClockTime);
                var path = Vehicle.Current.PathingTable[Vehicle.Next];
                foreach (var v in Status.VehiclesOnPath[path]) 
                    // moving in new vehicle may update the speeds for existing vehicles
                    Schedule(new Reach { Vehicle = v }, v.TimeToReach.Value); 
                Status.PathUtils[path].ObserveChange(1, ClockTime);
            }
            else Execute(new Reach { Vehicle = Vehicle });
            
            Status.Log("{0}\tMove: {1}", ClockTime.ToLongTimeString(), Vehicle.GetStr_Status());
            Status.Log(Status.GetStr_VehiclesOnPath());
            //Console.ReadKey();
        }
    }
}
