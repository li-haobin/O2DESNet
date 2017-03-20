using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;
using static Test.ControlPoint;
using Test;

namespace Test
{
    public class Vehicle : Load<Vehicle.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
        }
        
        //public new Statics Config { get { return (Statics)base.Config; } } // for inheritated component

        #endregion

        #region Sub-Components
        //private Server<TLoad> Server { get; set; }        
        #endregion

        #region Dynamics
        //public int Occupancy { get { return Server.Occupancy; } }  
        //public PathMover PathMover { get; private set; }
        public List<ControlPoint> Targets { get; private set; } = new List<ControlPoint>();
        public virtual double Speed { get; set; } = 5;
        public virtual double Acceleration { get; set; } = 0;      
        public ControlPoint Position { get; set; } = null;
        public Dictionary<int, double> Mileage { get; set; } = new Dictionary<int, double>() { { 0, 0 } }; // mileage with index 0, can't be reset
        public Path TravellingOn { get; set; } = null;
        public DateTime TimeStamp { get; set; } = DateTime.MinValue;
        #endregion

        #region Events
        private abstract class EventOfVehicle : Event { internal Vehicle This { get; set; } } // event adapter 
        private class SetAccelerationEvent : EventOfVehicle
        {
            public double Acceleration { get; set; }            
            public override void Invoke()
            {
                This.Acceleration = Acceleration;
            }
            public override string ToString() { return string.Format("{0}_SetAcceleration", This); }
        }

        private class SetSpeedEvent : EventOfVehicle
        {  
            public double Speed { get; set; }   
            public override void Invoke()
            {
                This.Speed = Speed;
            }
            public override string ToString() { return string.Format("{0}_SetSpeed", This); }
        }

        private class SetTargetsEvent : EventOfVehicle
        {
            public List<ControlPoint> Targets { get; set; }
            public override void Invoke()
            {
                This.Targets = Targets;
            }
            public override string ToString() { return string.Format("{0}_SetTargets", This); }
        }

        private class ResetMileageEvent : EventOfVehicle
        {
            public int Index { get; set; }
            public override void Invoke()
            {
                if (Index == 0) throw new Exception("Mileage with Index 0 cannot be reset.");
                if (!This.Mileage.ContainsKey(Index)) This.Mileage.Add(Index, 0);
                else This.Mileage[Index] = 0;
            }
            public override string ToString() { return string.Format("{0}_ResetMileage", This); }
        }

        private class UpdateMileageEvent : EventOfVehicle
        {
            public double Increment { get; set; }               
            public override void Invoke()
            {
                foreach (var idx in This.Mileage.Keys) This.Mileage[idx] += Increment;
            }
            public override string ToString() { return string.Format("{0}_UpdateMileage", This); }
        }

        private class AddTargetsEvent : EventOfVehicle
        {            
            public List<ControlPoint> Targets { get; set; }
            public override void Invoke()
            {
                if (This.Targets == null) { This.Targets = new List<ControlPoint>(Targets); }
                else This.Targets.AddRange(Targets);
            }
            public override string ToString() { return string.Format("{0}_AddTargets", This); }
        }

        private class SetTimeStampEvent : EventOfVehicle
        {           
            public DateTime TimeStamp { get; set; }           
            public override void Invoke()
            {
                This.TimeStamp = TimeStamp;
            }
            public override string ToString() { return string.Format("{0}_SetTimeStamp", This); }
        }

        private class SetPositionEvent : EventOfVehicle
        {           
            public ControlPoint Position { get; set; }           
            public override void Invoke()
            {
                This.Position = Position;
                if (This.Targets.Contains(Position)) This.Targets.Remove(Position);
            }
            public override string ToString() { return string.Format("{0}_SetPosition", This); }
        }

        private class SetTravellingPathEvent : EventOfVehicle
        {          
            public Path TravellingOn { get; set; }           
            public override void Invoke()
            {
                This.TravellingOn = TravellingOn;
            }
            public override string ToString() { return string.Format("{0}_SetTravellingPath", This); }
        }

        #endregion

        #region Input Events - Getters
        public Event SetAcceleration(double acceleration) { return new SetAccelerationEvent { This = this, Acceleration = acceleration }; }
        public Event SetSpeed(double speed) { return new SetSpeedEvent { This = this, Speed = speed }; }
        public Event SetTargets(List<ControlPoint> targets) { return new SetTargetsEvent { This = this, Targets = targets }; }
        public Event ResetMileage(int index) { return new ResetMileageEvent { This = this, Index = index }; }
        public Event UpdateMileage(double increment) { return new UpdateMileageEvent { This = this, Increment = increment }; }
        public Event AddTargets(List<ControlPoint> targets) { return new AddTargetsEvent { This = this, Targets = targets }; }
        public Event SetTimeStamp(DateTime timeStamp) { return new SetTimeStampEvent { This = this, TimeStamp = timeStamp }; }
        public Event SetPosition(ControlPoint position) { return new SetPositionEvent { This = this, Position = position }; }
        public Event SetTravellingPath(Path travellingOn) { return new SetTravellingPathEvent { This = this, TravellingOn = travellingOn }; }       
        #endregion

        #region Output Events - Reference to Getters
        //public List<Func<TLoad, Event>> OnOutput { get; private set; } = new List<Func<TLoad, Event>>();
        public List<Func<Event>> OnComplete { get; private set; } = new List<Func<Event>>();
        #endregion

        #region Exeptions
        public class VehicleStatusException : Exception
        {
            public VehicleStatusException(string message) : base(string.Format("Vechile Status Exception: {0}", message)) { }
        }
        #endregion

        public Vehicle(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Vehicle";
            //InitEvents.Add(new);
        }

        public override void WarmedUp(DateTime clockTime)
        {
            throw new NotImplementedException();
        }

        public override void Log(Event evnt) { }

        public override void WriteToConsole(DateTime? clockTime)
        {
            Console.WriteLine("Id:\t#{0}", Id);
            Console.WriteLine("Speed:\t{0}", Speed);
        }
    }
}
