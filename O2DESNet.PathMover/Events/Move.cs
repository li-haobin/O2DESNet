using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    public class Move<TScenario,TStatus> : Event<TScenario, TStatus>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
    {
        public PMStatus PMStatus { get; set; }
        public Vehicle Vehicle { get; set; }

        public override void Invoke()
        {
            if (!PMStatus.Vehicles.Contains(Vehicle)) return;
            if (Vehicle.Origin == null)
            {
                Vehicle.Origin = Vehicle.Current;
                Vehicle.DepartureTime = ClockTime;
            }
            if (!Vehicle.Current.Equals(Vehicle.Targets.First()))
            {
                Vehicle.Move(Vehicle.Current.RoutingTable[Vehicle.Targets.First()], ClockTime);                
                var path = Vehicle.Current.PathingTable[Vehicle.Next];

                if (Vehicle.OnMove != null) Vehicle.OnMove();
                if (!PMStatus.Vehicles.Contains(Vehicle)) return;

                foreach (var v in PMStatus.VehiclesOnPath[path])
                    // moving in new vehicle may update the speeds for existing vehicles
                    Schedule(new Reach<TScenario, TStatus>(PMStatus, v), v.TimeToReach.Value);
                PMStatus.PathUtils[path].ObserveChange(1, ClockTime);
            }
            else Execute(new Reach<TScenario, TStatus>(PMStatus, Vehicle));
            
            Status.Log("{0}\tMove: {1}", ClockTime.ToLongTimeString(), Vehicle.GetStr_Status());
            //Status.Log(Dynamics.GetStr_VehiclesOnPath());
        }
    }
}
