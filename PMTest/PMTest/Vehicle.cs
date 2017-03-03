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
            public string Name { get; set; }
            public double Speed { get; set; }//static or dynamic?

            /// <summary>
            /// Timestamps are recorded for tracking the movement of the vehicle
            /// </summary>
            public bool KeepTrack { get; set; }
            public double Accelerate { get; set; }// statics or dynamic?

            public double Length { get; set; } = 3.95;
            public double SafetyLength { get { return Length * (1 + DistanceSafetyFactor); } }
            public double Width { get; set; } = 1.67;
            public double DistanceSafetyFactor { get; set; } = 0.15;

            public string Color { get; set; } = "green";

            public Statics() { Name = Guid.NewGuid().ToString().ToUpper().Substring(0, 4); }
        }
        //public new Statics Config { get { return (Statics)base.Config; } } // for inheritated component

        #endregion

        #region Sub-Components
        //private Server<TLoad> Server { get; set; }        
        #endregion

        #region Dynamics
        //public int Occupancy { get { return Server.Occupancy; } }  
        public PathMover PathMover { get; private set; }
        public virtual double Speed { get; set; }
        public virtual double Accelerate { get; }//have not set yet
        public List<ControlPoint> Targets { get; private set; } = new List<ControlPoint>();
        public ControlPoint Current { get; set; } = null;
        public ControlPoint NextControlPoint { get; set; }//have not defined yet
        public Segment SegmentToNextControlPoint { get;  }//have not defined yet
        public Segment NextSegment { get; }//have not defined yet

        public enum State { Travelling,Parking,Off }
        public List<Tuple<double, State>> StateHistory { get; private set; } = new List<Tuple<double, State>> { new Tuple<double, State>(0, State.Off) };
        public void LogState(DateTime clockTime, State state) { StateHistory.Add(new Tuple<double, State>((clockTime - PathMover.StartTime).TotalSeconds, state)); }
        public void ResetStateHistory() { StateHistory = new List<Tuple<double, State>> { new Tuple<double, State>(0, StateHistory.Last().Item2) }; }
        #endregion

        #region Events
        private abstract class EventOfVehicle : Event { internal Vehicle This { get; set; } } // event adapter 
                                                                                              //private class InternalEvent : EventOfVehicle
                                                                                              //{
                                                                                              //    internal TLoad Load { get; set; }
                                                                                              //    public override void Invoke() {  }
                                                                                              //}

        private class PutOnEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal PutOnEvent(ControlPoint controlPoint, Vehicle vehicle)
            {
                ControlPoint = controlPoint;
                Vehicle = vehicle;
                Vehicle.PathMover = ControlPoint.PathMover;
            }
            public override void Invoke() { }
            
            public override string ToString() { return string.Format("{0}_PutOn", Vehicle); }
        }

        private class PutOffEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal PutOffEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke() { }
            public override string ToString() { return string.Format("{0}_PutOff", Vehicle); }
        }

        private class MoveEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal MoveEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke() { }
            public override string ToString() { return string.Format("{0}_Move", Vehicle); }
        }

        private class ReachEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal ReachEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                var next = Vehicle.NextControlPoint;
                Vehicle.Log(this);
                Execute(next.Reach(Vehicle));
                Vehicle.Current = next;

                while (Vehicle.Current == Vehicle.Targets.FirstOrDefault()) Vehicle.Targets.RemoveAt(0);
            }
            public override string ToString() { return string.Format("{0}_Reach", Vehicle); }
        }

        private class MoveToEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public List<ControlPoint> Targets { get; private set; }
            internal MoveToEvent(Vehicle vehicle, List<ControlPoint> targets)
            {
                Vehicle = vehicle;
                Targets = targets;
            }
            public override void Invoke() { }
            public override string ToString() { return string.Format("{0}_MoveTo", Vehicle); }
        }

        private class ClearTargetsEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal ClearTargetsEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                if (Vehicle.Targets.Count > 0) Vehicle.Targets = new List<ControlPoint> { Vehicle.Targets.First() };
            }
            public override string ToString() { return string.Format("{0}_ClearTargets", Vehicle); }
        }

        private class CompleteEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal CompleteEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                Vehicle.LogState(ClockTime, Vehicle.State.Parking);
                foreach (var evnt in Vehicle.OnComplete) Execute(evnt());
            }
            public override string ToString() { return string.Format("{0}_Complete", Vehicle); }
        }

        private class UpdatePositionsInSegmentEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public UpdatePositionsInSegmentEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke() { }
        }


        #endregion

        #region Input Events - Getters
        //public Event Input(TLoad load) { return new InternalEvent { This = this, Load = load }; }
        public Event PutOn(ControlPoint current) { return new PutOnEvent(current, this); }
        public Event PutOff() { return new PutOffEvent(this); }
        /// <summary>
        /// Move the vehicle to a list of targets in sequence, append to the list if there are existing targets
        /// </summary>
        public Event MoveTo(List<ControlPoint> targets) { return new MoveToEvent(this, targets); }
        /// <summary>
        /// Clear all targets except the first one which is being executed.
        /// </summary>
        public Event ClearTargets() { return new ClearTargetsEvent(this); }

        // Moving from control point to control point
        internal Event Move() { return new MoveEvent(this); }
        internal Event Reach() { return new ReachEvent(this); }
        internal Event Complete() { return new CompleteEvent(this); }
        internal Event UpdatePositionsInSegment() { return new UpdatePositionsInSegmentEvent(this); }
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
        }

        public override void WarmedUp(DateTime clockTime)
        {
            throw new NotImplementedException();
        }

        public override void Log(Event evnt) { }

        public override void WriteToConsole(DateTime? clockTime) { }
    }
}
