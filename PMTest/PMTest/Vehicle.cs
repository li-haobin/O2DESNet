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
            public int index;
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
        public Dictionary<int, double> Mileage { get; set; } = new Dictionary<int, double>() { { 0, 0 } };
        public Path TravellingOn { get; set; } = null;
        public DateTime TimeStamp { get; set; } = new DateTime(2017, 1, 1, 0, 0, 0);

        #endregion

        #region Events
        private abstract class EventOfVehicle : Event { internal Vehicle This { get; set; } } // event adapter 
                                                                                              //private class InternalEvent : EventOfVehicle
                                                                                              //{
                                                                                              //    internal TLoad Load { get; set; }
                                                                                              //    public override void Invoke() {  }
                                                                                              //}

        private class SetAccelerationEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public double Acceleration { get; set; }
            internal SetAccelerationEvent(double acceleration, Vehicle vehicle)
            {
                Vehicle = vehicle;
                Acceleration = acceleration;
                this.Invoke();
            }
            public override void Invoke()
            {
                Vehicle.Acceleration = Acceleration;
            }
            public override string ToString()
            {
                return string.Format("{0}_SetAcceleration",Vehicle);
            }
        }

        private class SetSpeedEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public double Speed { get; set; }
            public SetSpeedEvent(double speed, Vehicle vehicle)
            {
                Speed = speed;
                Vehicle = vehicle;
                this.Invoke();
            }
            public override void Invoke()
            {
                Vehicle.Speed = Speed;
            }
            public override string ToString()
            {
                return string.Format("{0}_SetSpeed",Vehicle);
            }
        }

        private class SetTargetsEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public List<ControlPoint> Targets { get; set; }
            public SetTargetsEvent(List<ControlPoint> targets, Vehicle vehicle)
            {
                Targets = targets;
                Vehicle = vehicle;
                this.Invoke();
            }
            public override void Invoke()
            {
                Vehicle.Targets = Targets;
            }
            public override string ToString()
            {
                return string.Format("{0}_SetTargets",Vehicle);
            }
        }

        private class SetMileageEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public Dictionary<int, double> Mileage { get; set; }
            internal SetMileageEvent(Dictionary<int, double> mileage, Vehicle vehicle)
            {
                Vehicle = vehicle;
                Mileage = mileage;
            }
            public override void Invoke()
            {
                Vehicle.Mileage = Mileage;
            }
            public override string ToString()
            {
                return string.Format("{0}_SetMileage",Vehicle);
            }
        }

        private class ResetMileageEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal ResetMileageEvent(Vehicle vehicle)
            {
                Vehicle = vehicle;
                this.Invoke();
            }
            public override void Invoke()
            {
                Vehicle.Mileage = new Dictionary<int, double>() { { 0,0} };
            }
            public override string ToString()
            {
                return string.Format("{0}_ResetMileage",Vehicle);
            }
        }

        private class UpdateMileageEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public double Mileage { get; set; }
            internal UpdateMileageEvent(double mileage, Vehicle vehicle)
            {
                Vehicle = vehicle;
                Mileage = mileage;
                this.Invoke();            
            }
            public override void Invoke()
            {               
                Vehicle.Mileage.Add(Vehicle.Mileage.Count, Mileage);
            }
            public override string ToString()
            {
                return string.Format("{0}_UpdateMileage",Vehicle);
            }
        }

        private class AddTargetsEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public ControlPoint NewControlpoint { get; set; }
            internal AddTargetsEvent(ControlPoint newcontrolpoint, Vehicle vehicle)
            {
                Vehicle = vehicle;
                NewControlpoint = newcontrolpoint;
                this.Invoke();
            }
            public override void Invoke()
            {
                if (Vehicle.Targets == null){ Vehicle.Targets = new List<ControlPoint>(); }
                Vehicle.Targets.Add(NewControlpoint);
            }
            public override string ToString()
            {
                return string.Format("{0}_AddTargets",Vehicle);
            }
        }

        private class SetTimeStampEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public DateTime TimeStamp { get; set; }
            internal SetTimeStampEvent(DateTime timestamp, Vehicle vehicle)
            {
                TimeStamp = timestamp;
                Vehicle = vehicle;
                this.Invoke();
            }
            public override void Invoke()
            {
                Vehicle.TimeStamp = TimeStamp;
            }
            public override string ToString()
            {
                return string.Format("{0}_SetTimeStamp",Vehicle);
            }
        }

        private class SetPositionEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public ControlPoint Position { get; set; }
            internal SetPositionEvent(ControlPoint position, Vehicle vehicle)
            {
                Position = position;
                Vehicle = vehicle;
                this.Invoke();
            }
            public override void Invoke()
            {
                Vehicle.Position = Position;
                if (Vehicle.Targets.Contains(Position)) Vehicle.Targets.Remove(Position);
            }
            public override string ToString()
            {
                return string.Format("{0}_SetPosition",Vehicle);
            }
        }

        private class SetTravellingPathEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public Path TravellingOn { get; set; }
            internal SetTravellingPathEvent(Path travellingon, Vehicle vehicle)
            {
                Vehicle = vehicle;
                TravellingOn = travellingon;
                this.Invoke();
            }
            public override void Invoke()
            {
                Vehicle.TravellingOn = TravellingOn;
            }
            public override string ToString()
            {
                return string.Format("{0}_SetTravellingPath",Vehicle);
            }
        }

        #endregion

        #region Input Events - Getters
        //public Event Input(TLoad load) { return new InternalEvent { This = this, Load = load }; }

        public Event SetAcceleration(double Acceleration) { return new SetAccelerationEvent(Acceleration, this); }
        public Event SetSpeed(double Speed) { return new SetSpeedEvent(Speed, this); }
        public Event SetTargets(List<ControlPoint> Targets) { return new SetTargetsEvent(Targets, this); }
        public Event SetMileage(Dictionary<int, double> Mileage) { return new SetMileageEvent(Mileage, this); }
        public Event ResetMileage() { return new ResetMileageEvent(this); }
        public Event UpdateMileage(double Mileage) { return new UpdateMileageEvent(Mileage, this); }
        public Event AddTargets(ControlPoint NewControlpoint) { return new AddTargetsEvent(NewControlpoint, this); }
        public Event SetTimeStamp(DateTime TimeStamp) { return new SetTimeStampEvent(TimeStamp, this); }
        public Event SetPosition(ControlPoint Position) { return new SetPositionEvent(Position, this); }
        public Event SetTravellingPath(Path TravellingOn) { return new SetTravellingPathEvent(TravellingOn, this); }
       
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

        public override void WriteToConsole(DateTime? clockTime) { }
    }
}
