using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Dynamics
{
    internal class Status
    {
        internal Simulator _sim;   
        public Vehicle[] AllVehicles { get; private set; }
        public HashSet<Vehicle> OffVehicles { get; private set; }
        public Dictionary<ControlPoint, HashSet<Vehicle>> IncomingVehicles { get; private set; }
        public Dictionary<ControlPoint, HashSet<Vehicle>> OutgoingVehicles { get; private set; }
        public Dictionary<Vehicle, Dictionary<Vehicle, DateTime[]>> Conflicts_PassOver { get; private set; }
        public Dictionary<Vehicle, Dictionary<Vehicle, DateTime>> Conflicts_CrossOver { get; private set; }
        // for analysis
        public Dictionary<ControlPoint, HourCounter> VehicleCounters { get; private set; }
        public int Count_PassOvers = 0;
        public int Count_CrossOvers = 0;
        public int Count_FailureToScheduleWithinBuffer = 0;

        internal Status(Simulator simulation)
        {
            _sim = simulation;
            AllVehicles = _sim.Scenario.NumsVehicles.SelectMany(item => Enumerable.Range(0, item.Value).Select(i => new Vehicle(this, item.Key))).ToArray();
            OffVehicles = new HashSet<Vehicle>(AllVehicles);
            IncomingVehicles = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HashSet<Vehicle>());
            OutgoingVehicles = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HashSet<Vehicle>());
            Conflicts_PassOver = AllVehicles.ToDictionary(v => v, v => new Dictionary<Vehicle, DateTime[]>());
            Conflicts_CrossOver = AllVehicles.ToDictionary(v => v, v => new Dictionary<Vehicle, DateTime>());
            VehicleCounters = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HourCounter(_sim));
        }

        
    }
}
