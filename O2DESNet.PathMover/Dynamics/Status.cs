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
        public Dictionary<ControlPoint, HashSet<Vehicle>> IncomingVehicles { get; private set; }
        public Dictionary<ControlPoint, HashSet<Vehicle>> OutgoingVehicles { get; private set; }
        public HashSet<Vehicle> OffVehicles { get; private set; }

        internal Status(Simulator simulation)
        {
            _sim = simulation;
            AllVehicles = _sim.Scenario.NumsVehicles.SelectMany(item => Enumerable.Range(0, item.Value).Select(i => new Vehicle(item.Key))).ToArray();
            IncomingVehicles = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HashSet<Vehicle>());
            OutgoingVehicles = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HashSet<Vehicle>());
            OffVehicles = new HashSet<Vehicle>(AllVehicles);
        }

        internal void PutOn(Vehicle vehicle, ControlPoint controlPoint)
        {
            if (vehicle.On)
                throw new Exceptions.VechicleStatusError(vehicle, "The vehicle is on already.");
            vehicle.On = true;
            vehicle.Current = controlPoint;
            vehicle.Towards = null;
            vehicle.EndingSpeed = 0;
            vehicle.TargetSpeed = 0;
            vehicle.Acceleration = 0;
            OffVehicles.Remove(vehicle);
            OutgoingVehicles[controlPoint].Add(vehicle);
        }
        internal void PutOff(Vehicle vehicle)
        {
            OutgoingVehicles[vehicle.Current].Remove(vehicle);
            IncomingVehicles[vehicle.Towards].Remove(vehicle);
            vehicle.Current = null;
            vehicle.Towards = null;
            vehicle.On = false;
            OffVehicles.Add(vehicle);
        }
        internal void MoveTowards(Vehicle vehicle, ControlPoint towards, double? targetSpeed = null, double? acceleration = null)
        {            
            vehicle.DistanceToTravel = vehicle.Current.GetDistanceTo(towards);
            IncomingVehicles[towards].Add(vehicle);
            vehicle.Towards = towards;
            vehicle.SpeedControl(targetSpeed, acceleration);
            vehicle.ForwardCalculate();
        }
    }
}
