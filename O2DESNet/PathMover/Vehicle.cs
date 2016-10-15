using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet
{
    public class Vehicle : Load<Vehicle.Statics>
    {
        #region Sub-Components
        #endregion

        #region Statics
        public class Statics : Scenario
        {
            public double Speed { get; set; }

            /// <summary>
            /// Timestamps are recorded for tracking the movement of the vehicle
            /// </summary>
            public bool KeepTrack { get; set; }
        }
        #endregion

        #region Dynamics
        public virtual double Speed { get { return Category.Speed; } }
        public List<ControlPoint> Targets { get; private set; } = new List<ControlPoint>();
        public ControlPoint Current { get; private set; } = null;        
        public ControlPoint Next
        {
            get
            {
                if (Targets.Count > 0)
                    return Current.PathMover.ControlPoints[Current.Config.RoutingTable[Targets.First().Config]];
                else return null;
            }
        }
        public Path PathToNext
        {
            get
            {
                if (Targets.Count > 0 && Targets.First() != Current)
                    return Current.PathMover.Paths[Current.Config.GetPathFor(Targets.First().Config)];
                else return null;
            }
        }
        #endregion

        #region Events
        private class PutOnEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public ControlPoint ControlPoint { get; private set; }
            internal PutOnEvent(Vehicle vehicle, ControlPoint controlPoint)
            {
                Vehicle = vehicle;
                ControlPoint = controlPoint;
            }
            public override void Invoke()
            {
                if (Vehicle.Current != null) throw new VehicleStatusException("'Current' must be null on PutOn event.");                
                Vehicle.Current = ControlPoint;
                Execute(new ControlPoint.PutOnEvent(ControlPoint, Vehicle));
            }
            public override string ToString() { return string.Format("{0}_PutOn", Vehicle); }
        }
        private class PutOffEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public ControlPoint ControlPoint { get; private set; }
            internal PutOffEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                if (Vehicle.Targets.Count > 0) throw new VehicleStatusException("'Targets' must be empty on PutOff event.");
                if (Vehicle.Current == null) throw new VehicleStatusException("'Current' cannot be null on PutOff event.");
                if (Vehicle.Category.KeepTrack) Vehicle.Log(this);
                Vehicle.Current = null;
            }
            public override string ToString() { return string.Format("{0}_PutOff", Vehicle); }
        }
        private class MoveEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal MoveEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                Vehicle.Log(this);
                Execute(Vehicle.Current.MoveOut(Vehicle));
                Execute(Vehicle.Next.MoveIn(Vehicle));
            }
            public override string ToString() { return string.Format("{0}_Move", Vehicle); }
        }
        private class ReachEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal ReachEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                var next = Vehicle.Next;
                Vehicle.Log(this);
                Execute(Vehicle.Current.Leave(Vehicle));
                Execute(next.Reach(Vehicle));
                Vehicle.Current = next;

                if (Vehicle.Current == Vehicle.Targets.FirstOrDefault()) Vehicle.Targets.RemoveAt(0);
                if (Vehicle.Targets.Count == 0) Execute(new CompleteEvent(Vehicle));
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
            public override void Invoke()
            {
                if (Vehicle.Current == null) throw new VehicleStatusException("'Current' cannot be null on MoveTo event.");
                Vehicle.Log(this);
                Vehicle.Targets.AddRange(Targets);

                if (Vehicle.Targets.First() == Vehicle.Current) Execute(Vehicle.Reach());
                else Execute(Vehicle.PathToNext.Move(Vehicle));

            }
            public override string ToString() { return string.Format("{0}_MoveTo", Vehicle); }
        }
        private class CompleteEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal CompleteEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                Vehicle.Log(this);
                foreach (var evnt in Vehicle.OnComplete) Execute(evnt());
            }
            public override string ToString() { return string.Format("{0}_Complete", Vehicle); }
        }
        #endregion

        #region Input Events - Getters
        public Event PutOn(ControlPoint current) { return new PutOnEvent(this, current); }
        public Event PutOff() { return new PutOffEvent(this); }
        public Event MoveTo(List<ControlPoint> targets) { return new MoveToEvent(this, targets); }
        
        // Moving from control point to control point
        internal Event Move() { return new MoveEvent(this); }
        internal Event Reach() { return new ReachEvent(this); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Event>> OnComplete { get; private set; }
        #endregion

        #region Exeptions
        public class VehicleStatusException : Exception
        {
            public VehicleStatusException(string message) : base(string.Format("Vechile Status Exception: {0}", message)) { }
        }
        #endregion

        public Vehicle(Statics category, int seed, string tag = null) : base(category, seed, tag)
        {
            Name = "Veh";

            // initialize for output events
            OnComplete = new List<Func<Event>>();
        }

        public override void Log(Event evnt) { if (Category.KeepTrack) base.Log(evnt); }

        public override void WarmedUp(DateTime clockTime) { }
    }
}
