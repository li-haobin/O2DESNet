using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet
{
    public class ControlPoint : Component<ControlPoint.Statics>
    {
        #region Sub-Components
        #endregion

        #region Statics
        public class Statics : Scenario
        {
            /// <summary>
            /// The PathMover system it belongs to
            /// </summary>
            public PathMover.Statics PathMover { get; private set; }
            /// <summary>
            /// The index in the path mover system
            /// </summary>
            public int Index { get; private set; }
            /// <summary>
            /// Check for the position on each path
            /// </summary>
            public Dictionary<Path.Statics, double> Positions { get; internal set; }
            /// <summary>
            /// Check for the next control point to visit, providing the destination
            /// </summary>
            public Dictionary<Statics, Statics> RoutingTable { get; internal set; }
            /// <summary>
            /// Check for the path to take, providing the next control point to visit
            /// </summary>
            public Dictionary<Statics, Path.Statics> PathingTable { get; set; }

            public Statics(PathMover.Statics pathMover)
            {
                PathMover = pathMover;
                Index = PathMover.ControlPoints.Count;
                Positions = new Dictionary<Path.Statics, double>();
            }

            /// <summary>
            /// Get distance to an adjacent control point
            /// </summary>
            public double GetDistanceTo(Statics next)
            {
                if (next.Equals(this)) return 0;
                if (!PathingTable.ContainsKey(next))
                    throw new Exception("Make sure the next control point is in pathing table.");
                var path = PathingTable[next];
                return Math.Abs(next.Positions[path] - Positions[path]);
            }

            /// <summary>
            /// Get the next path for reaching the target
            /// </summary>
            public Path.Statics GetPathFor(Statics target) { return PathingTable[RoutingTable[target]]; }
        }
        #endregion

        #region Dynamics
        public PathMover PathMover { get; internal set; }
        public HashSet<Vehicle> Outgoing { get; private set; }
        public HashSet<Vehicle> Incoming { get; private set; }
        public HashSet<Vehicle> At { get; private set; }
        #endregion

        #region Events
        private class MoveInEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal MoveInEvent(ControlPoint controlPoint, Vehicle vehicle)
            {
                ControlPoint = controlPoint;
                Vehicle = vehicle;
            }
            public override void Invoke() { ControlPoint.Incoming.Add(Vehicle); }
            public override string ToString() { return string.Format("{0}_MoveIn", ControlPoint); }
        }
        private class ReachEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal ReachEvent(ControlPoint controlPoint, Vehicle vehicle)
            {
                ControlPoint = controlPoint;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                if (!ControlPoint.Incoming.Contains(Vehicle)) throw new VehicleIsNotIncomingException();
                if (Vehicle.Category.KeepTrack) Vehicle.Log(this);
                ControlPoint.Incoming.Remove(Vehicle);
                ControlPoint.At.Add(Vehicle);
            }
            public override string ToString() { return string.Format("{0}_Reach", ControlPoint); }
        }
        private class LeaveEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal LeaveEvent(ControlPoint controlPoint, Vehicle vehicle)
            {
                ControlPoint = controlPoint;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                if (!ControlPoint.At.Contains(Vehicle)) throw new VehicleIsNotAtException();
                if (Vehicle.Category.KeepTrack) Vehicle.Log(this);
                ControlPoint.At.Remove(Vehicle);
                ControlPoint.Outgoing.Add(Vehicle);
            }
            public override string ToString() { return string.Format("{0}_Leave", ControlPoint); }
        }
        private class MoveOutEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal MoveOutEvent(ControlPoint controlPoint, Vehicle vehicle)
            {
                ControlPoint = controlPoint;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                if (!ControlPoint.Outgoing.Contains(Vehicle)) throw new VehicleIsNotOutgoingException();
                ControlPoint.Outgoing.Remove(Vehicle);
            }
            public override string ToString() { return string.Format("{0}_MoveOut", ControlPoint); }
        }
        #endregion

        #region Input Events - Getters
        public Event MoveIn(Vehicle vehicle) { return new MoveInEvent(this, vehicle); }
        public Event Reach(Vehicle vehicle) { return new ReachEvent(this, vehicle); }
        public Event Leave(Vehicle vehicle) { return new LeaveEvent(this, vehicle); }
        public Event MoveOut(Vehicle vehicle) { return new MoveOutEvent(this, vehicle); }
        #endregion

        #region Output Events - Reference to Getters
        #endregion

        #region Exeptions
        public class VehicleIsNotIncomingException : Exception
        {
            public VehicleIsNotIncomingException() : base("Please ensure the vehicle is incoming before execute 'Reach' event at the control point.") { }
        }
        public class VehicleIsNotAtException : Exception
        {
            public VehicleIsNotAtException() : base("Please ensure the vehicle is at the control point before execute 'StartMoveOut' event.") { }
        }
        public class VehicleIsNotOutgoingException : Exception
        {
            public VehicleIsNotOutgoingException() : base("Please ensure the vehicle is outgoing before execute 'Leave' event.") { }
        }
        #endregion

        public ControlPoint(Statics config, int seed = 0, string tag = null) : base(config, seed, tag)
        {
            Name = "ControlPoint";
            Outgoing = new HashSet<Vehicle>();
            Incoming = new HashSet<Vehicle>();
            At = new HashSet<Vehicle>();
        }

        public override void WarmedUp(DateTime clockTime) { }

        public override void WriteToConsole()
        {
            throw new NotImplementedException();
        }
    }
}
