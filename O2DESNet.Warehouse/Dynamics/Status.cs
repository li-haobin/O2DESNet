using O2DESNet.Warehouse.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.Dynamics
{
    internal class Status
    {
        private Simulator _sim;   
        public List<Picker> AllPicker { get; private set; }
        //public Dictionary<ControlPoint, HashSet<Vehicle>> IncomingVehicles { get; private set; }
        //public Dictionary<ControlPoint, HashSet<Vehicle>> OutgoingVehicles { get; private set; }
        //public HashSet<Vehicle> OffVehicles { get; private set; }
        //// for analysis
        //public Dictionary<ControlPoint, HourCounter> VehicleCounters { get; private set; }

        internal Status(Simulator simulation)
        {
            _sim = simulation;
            //AllVehicles = _sim.Scenario.NumPickers.SelectMany(item => Enumerable.Range(0, item.Value).Select(i => new Vehicle(item.Key))).ToArray();
            //IncomingVehicles = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HashSet<Vehicle>());
            //OutgoingVehicles = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HashSet<Vehicle>());
            //OffVehicles = new HashSet<Vehicle>(AllVehicles);
            //VehicleCounters = _sim.Scenario.ControlPoints.ToDictionary(cp => cp, cp => new HourCounter(_sim));
        }

        //internal void PutOn(Vehicle vehicle, ControlPoint controlPoint)
        //{
        //    if (vehicle.On)
        //        throw new Exceptions.VechicleStatusError(vehicle, "The vehicle is on already.");
        //    vehicle.On = true;
        //    vehicle.Current = controlPoint;
        //    vehicle.Towards = null;
        //    vehicle.EndingSpeed = 0;
        //    vehicle.TargetSpeed = 0;
        //    vehicle.Acceleration = 0;
        //    OffVehicles.Remove(vehicle);
        //    OutgoingVehicles[controlPoint].Add(vehicle);
        //    VehicleCounters[controlPoint].ObserveChange(1);
        //}
        //internal void PutOff(Vehicle vehicle)
        //{
        //    OutgoingVehicles[vehicle.Current].Remove(vehicle);
        //    IncomingVehicles[vehicle.Towards].Remove(vehicle);
        //    VehicleCounters[vehicle.Current].ObserveChange(-1);
        //    vehicle.Current = null;
        //    vehicle.Towards = null;
        //    vehicle.On = false;
        //    OffVehicles.Add(vehicle);
        //}
        //internal void MoveTowards(Vehicle vehicle, ControlPoint towards, double? targetSpeed = null, double? acceleration = null)
        //{
        //    //vehicle.HistoricalPath.Add(vehicle.Current);
        //    vehicle.DistanceToTravel = vehicle.Current.GetDistanceTo(towards);
        //    IncomingVehicles[towards].Add(vehicle);
        //    vehicle.Towards = towards;
        //    vehicle.SpeedControl(targetSpeed, acceleration);
        //    vehicle.ForwardCalculate();
        //    //Console.WriteLine("{0}:\tV{1} moved CP{2} -> CP{3} at S{4:F4}/{5:F4} & A{6}", _sim.ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), vehicle.Id, vehicle.Current.Id, vehicle.Towards.Id, vehicle.Speed, vehicle.TargetSpeed, vehicle.Acceleration);
        //    VehicleCounters[towards].ObserveChange(1);
        //}
        //internal void Reach(Vehicle vehicle)
        //{
        //    vehicle.Speed = vehicle.EndingSpeed;
        //    OutgoingVehicles[vehicle.Current].Remove(vehicle);
        //    VehicleCounters[vehicle.Current].ObserveChange(-1);
        //    IncomingVehicles[vehicle.Towards].Remove(vehicle);
        //    OutgoingVehicles[vehicle.Towards].Add(vehicle);            
        //    vehicle.Current = vehicle.Towards;
        //    vehicle.Towards = null;
        //}
    }
}
