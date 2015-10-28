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
        private Simulator _sim;   
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

        internal Status(Simulator simulation)
        {
            _sim = simulation;
            AllVehicles = _sim.Scenario.NumsVehicles.SelectMany(item => Enumerable.Range(0, item.Value).Select(i => new Vehicle(item.Key))).ToArray();
            OffVehicles = new HashSet<Vehicle>(AllVehicles);
            IncomingVehicles = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HashSet<Vehicle>());
            OutgoingVehicles = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HashSet<Vehicle>());
            Conflicts_PassOver = AllVehicles.ToDictionary(v => v, v => new Dictionary<Vehicle, DateTime[]>());
            Conflicts_CrossOver = AllVehicles.ToDictionary(v => v, v => new Dictionary<Vehicle, DateTime>());
            VehicleCounters = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HourCounter(_sim));
        }

        internal void PutOn(Vehicle vehicle, ControlPoint controlPoint)
        {
            if (vehicle.On)
                throw new Exceptions.VechicleStatusError(vehicle, "The vehicle is on already.");
            vehicle.On = true;
            vehicle.LastTime = _sim.ClockTime;
            vehicle.LastControlPoint = controlPoint;
            vehicle.NextControlPoint = null;
            vehicle.Speed_ToNextControlPoint = 0;
            vehicle.TargetSpeed = 0;
            vehicle.Acceleration = 0;
            OffVehicles.Remove(vehicle);
            OutgoingVehicles[controlPoint].Add(vehicle);
            VehicleCounters[controlPoint].ObserveChange(1);
        }
        internal void PutOff(Vehicle vehicle)
        {
            OutgoingVehicles[vehicle.LastControlPoint].Remove(vehicle);
            IncomingVehicles[vehicle.NextControlPoint].Remove(vehicle);
            VehicleCounters[vehicle.LastControlPoint].ObserveChange(-1);
            vehicle.LastControlPoint = null;
            vehicle.NextControlPoint = null;
            vehicle.On = false;
            OffVehicles.Add(vehicle);
        }
        internal void MoveToNext(Vehicle vehicle, ControlPoint nextControlPoint, double? targetSpeed = null, double? acceleration = null)
        {
            //vehicle.HistoricalPath.Add(vehicle.Current);
            vehicle.Distance_ToNextControlPoint = vehicle.LastControlPoint.GetDistanceTo(nextControlPoint);
            IncomingVehicles[nextControlPoint].Add(vehicle);
            vehicle.NextControlPoint = nextControlPoint;
            vehicle.SpeedControl(targetSpeed, acceleration);
            vehicle.ForwardCalculate();
            //Console.WriteLine("{0}:\tV{1} moved CP{2} -> CP{3} at S{4:F4}/{5:F4} & A{6}", _sim.ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), vehicle.Id, vehicle.Current.Id, vehicle.Towards.Id, vehicle.Speed, vehicle.TargetSpeed, vehicle.Acceleration);
            VehicleCounters[nextControlPoint].ObserveChange(1);
        }
        internal void Reach(Vehicle vehicle)
        {
            vehicle.Speed = vehicle.Speed_ToNextControlPoint;
            OutgoingVehicles[vehicle.LastControlPoint].Remove(vehicle);
            VehicleCounters[vehicle.LastControlPoint].ObserveChange(-1);
            IncomingVehicles[vehicle.NextControlPoint].Remove(vehicle);
            OutgoingVehicles[vehicle.NextControlPoint].Add(vehicle);
            vehicle.LastTime = _sim.ClockTime;           
            vehicle.LastControlPoint = vehicle.NextControlPoint;
            vehicle.NextControlPoint = null;
        }
        internal bool IdentifyConflicts_PassOver(Vehicle vehicle)
        {
            Conflicts_PassOver[vehicle] = new Dictionary<Vehicle, DateTime[]>();
            foreach (var v in OutgoingVehicles[vehicle.LastControlPoint].Intersect(IncomingVehicles[vehicle.NextControlPoint]))
                if (v != vehicle)
                {
                    var times = vehicle.GetTimes_PassOver(v);
                    if (times.Length > 0) Conflicts_PassOver[vehicle].Add(v, times);
                }
            return Conflicts_PassOver[vehicle].Count > 0;
        }
        internal bool IdentifyConflicts_CrossOver(Vehicle vehicle)
        {
            Conflicts_CrossOver[vehicle] = new Dictionary<Vehicle, DateTime>();
            foreach (var v in OutgoingVehicles[vehicle.NextControlPoint].Intersect(IncomingVehicles[vehicle.LastControlPoint]))
                Conflicts_CrossOver[vehicle].Add(v, vehicle.GetTime_CrossOver(v));                
            return Conflicts_CrossOver[vehicle].Count > 0;
        }
    }
}
