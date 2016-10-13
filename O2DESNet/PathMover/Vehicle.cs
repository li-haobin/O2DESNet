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
        public ControlPoint Target { get; private set; } = null;
        public ControlPoint Current { get; private set; } = null;        
        public ControlPoint Next
        {
            get
            {
                if (Target != null)
                    return Current.PathMover.ControlPoints[Current.Config.RoutingTable[Target.Config]];
                else return null;
            }
        }
        public Path PathToNext
        {
            get
            {
                if (Target != null)
                    return Current.PathMover.Paths[Current.Config.GetPathFor(Target.Config)];
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
                Vehicle.Log(this);
                Vehicle.Current = ControlPoint;
                ControlPoint.At.Add(Vehicle);
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
                if (Vehicle.Target != null) throw new VehicleStatusException("'Target' must be null on PutOff event.");
                if (Vehicle.Current == null) throw new VehicleStatusException("'Current' cannot be null on PutOff event.");
                if (Vehicle.Category.KeepTrack) Vehicle.Log(this);
                Vehicle.Current = null;
            }
            public override string ToString() { return string.Format("{0}_PutOff", Vehicle); }
        }
        private class MoveEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public ControlPoint Next { get; private set; }
            internal MoveEvent(Vehicle vehicle, ControlPoint next)
            {
                Vehicle = vehicle;
                Next = next;
            }
            public override void Invoke()
            {
                Vehicle.Log(this);
                Execute(Vehicle.Current.MoveOut(Vehicle));
                Execute(Next.MoveIn(Vehicle));
            }
            public override string ToString() { return string.Format("{0}_Move", Vehicle); }
        }
        private class ReachEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public ControlPoint Next { get; private set; }
            internal ReachEvent(Vehicle vehicle, ControlPoint next)
            {
                Vehicle = vehicle;
                Next = next;
            }
            public override void Invoke()
            {
                Vehicle.Log(this);
                Execute(Vehicle.Current.Leave(Vehicle));
                Execute(Next.Reach(Vehicle));
                Vehicle.Current = Next;
            }
            public override string ToString() { return string.Format("{0}_Move", Vehicle); }
        }
        private class DepartEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public ControlPoint Target { get; private set; }
            internal DepartEvent(Vehicle vehicle, ControlPoint target)
            {
                Vehicle = vehicle;
                Target = target;
            }
            public override void Invoke()
            {
                if (Vehicle.Current == null) throw new VehicleStatusException("'Current' cannot be null on Depart event.");
                Vehicle.Log(this);
                Vehicle.Target = Target;
            }
            public override string ToString() { return string.Format("{0}_Depart", Vehicle); }
        }
        private class ArriveEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal ArriveEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                if (Vehicle.Target == null) throw new VehicleStatusException("'Target' ControlPoint cannot be null on Arrive event");
                Vehicle.Log(this);
                Vehicle.Current = Vehicle.Target;
                Vehicle.Target = null;
            }
            public override string ToString() { return string.Format("{0}_Arrive", Vehicle); }
        }
        #endregion

        #region Input Events - Getters
        public Event PutOn(ControlPoint current) { return new PutOnEvent(this, current); }
        public Event PutOff() { return new PutOffEvent(this); }
        public Event Depart(ControlPoint target) { return new DepartEvent(this, target); }
        public Event Arrive() { return new ArriveEvent(this); }
        public Event Move(ControlPoint next) { return new MoveEvent(this, next); }
        public Event Reach(ControlPoint next) { return new ReachEvent(this, next); }
        #endregion

        #region Output Events - Reference to Getters
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
        }

        public override void Log(Event evnt) { if (Category.KeepTrack) base.Log(evnt); }

        public override void WarmedUp(DateTime clockTime) { }
    }
}
