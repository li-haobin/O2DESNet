using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    public class Reach<TScenario, TStatus> : Event<TScenario, TStatus>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
    {
        public PMStatus PMStatus { get; private set; }
        public Vehicle Vehicle { get; private set; }

        public Reach(PMStatus pmStatus, Vehicle vehicle)
        {
            PMStatus = pmStatus;
            Vehicle = vehicle;
            Vehicle.ReachEventHashCode = GetHashCode();
        }

        public override void Invoke()
        {            
            if (Vehicle.Targets.Count == 0) return;
            if (Vehicle.Next != null) // in case vehicle is not moving, skip
            {
                if (GetHashCode() != Vehicle.ReachEventHashCode) return; // for change of speed
                var path = Vehicle.Current.PathingTable[Vehicle.Next];
                Vehicle.Reach(ClockTime);
                PMStatus.PathUtils[path].ObserveChange(-1, ClockTime);
                Status.Log("{0}\tReach: {1}", ClockTime.ToLongTimeString(), Vehicle.GetStr_Status());
            }
            if (Vehicle.Current.Equals(Vehicle.Targets.First())) Vehicle.Targets.RemoveAt(0);
            if (Vehicle.OnReach != null) Vehicle.OnReach();
            if (Vehicle.Targets.Count == 0)
            {
                Vehicle.Origin = null;
                if (Vehicle.OnCompletion != null) Vehicle.OnCompletion();
            }
            else Execute(new Move<TScenario, TStatus> { PMStatus = PMStatus, Vehicle = Vehicle });
        }
    }
}
