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
        public PMStatus PMStatus { get; set; }
        public Vehicle Vehicle { get; set; }
        
        protected override void Invoke()
        {
            if (Vehicle.Targets.Count == 0) return;
            if (Vehicle.Next != null) // in case vehicle is not moving, skip
            {
                if (!ClockTime.Equals(Vehicle.TimeToReach)) return; // for change of speed
                var path = Vehicle.Current.PathingTable[Vehicle.Next];
                Vehicle.Reach(ClockTime);
                PMStatus.PathUtils[path].ObserveChange(-1, ClockTime);
                Status.Log("{0}\tReach: {1}", ClockTime.ToLongTimeString(), Vehicle.GetStr_Status());
            }
            if (Vehicle.Current.Equals(Vehicle.Targets.First())) Vehicle.Targets.RemoveAt(0);
            if (Vehicle.Targets.Count==0) Vehicle.OnCompletion();
            else Execute(new Move<TScenario, TStatus> { PMStatus = PMStatus, Vehicle = Vehicle });

            PMStatus.Changed = true;
        }
    }
}
