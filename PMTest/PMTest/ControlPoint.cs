using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace Test
{
    public class ControlPoint : Component<ControlPoint.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            //public new Statics Config { get { return (Statics)base.Config; } } // for inheritated component

            /// <summary>
            /// The PathMover system it belongs to
            /// </summary>
            public PathMover.Statics PathMover { get; private set; }
            //<summery>
            //The index in the path mover system
            //</summery>
            public int Index { get; private set; }
            /// <summary>
            /// Check for the position on each path
            /// </summary>
            public Dictionary<Segment.Statics, double> Positions { get; internal set; } // have not well defined yet

            public Statics(PathMover.Statics pathMover)
            {
                PathMover = pathMover;
                Index = PathMover.ControlPoints.Count;
                Positions = new Dictionary<Segment.Statics, double>();
            }
        }

        #endregion

        #region Sub-Components
        //private Server<TLoad> Server { get; set; }              
        #endregion

        #region Dynamics
        //public int Occupancy { get { return Server.Occupancy; } }  
        public PathMover PathMover { get; internal set; }
        public bool Locked { get; private set; } = false;// needed?
        public Vehicle At { get; internal set; }
        public HourCounter HourCounter { get; private set; }
        internal HashSet<Vehicle> Outgoing { get; private set; }
        internal HashSet<Vehicle> Incoming { get; private set; }
        internal HashSet<Vehicle> Onposition { get; private set; }
        /// <summary>
        /// Check if the control point is accessible via the given path
        /// </summary>
        public bool Accessible() { return true; }// have not defined yet
        internal List<Segment> IncomingSegments { get; private set; }
        internal List<Segment> OutgoingSegments { get; private set; }
        #endregion

        #region Events
        private abstract class EventOfControlPoint : Event { internal ControlPoint This { get; set; } } // event adapter 
                                                                                                //private class InternalEvent : EventOfPath
                                                                                                //{
                                                                                                //    internal TLoad Load { get; set; }
                                                                                                //    public override void Invoke() {  }
                                                                                                //}
        private class TravelToEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal TravelToEvent (ControlPoint controlpoint, Vehicle vehicle)
            {
                ControlPoint = controlpoint;
                Vehicle = vehicle;
            }
            public override void Invoke() 
            {
                throw new NotImplementedException();
            }
            public override string ToString()
            {
                return base.ToString();
            }
        }
        private class TravelFromEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal TravelFromEvent (ControlPoint controlpoint, Vehicle vehicle)
            {
                ControlPoint = controlpoint;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                throw new NotImplementedException();
            }
            public override string ToString()
            {
                return base.ToString();
            }
        }
        private class ReachEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; set; }
            internal ReachEvent (ControlPoint controlpoint, Vehicle vehicle)
            {
                ControlPoint = controlpoint;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                throw new NotImplementedException();
            }
            public override string ToString()
            {
                return base.ToString();
            }
        }
        private class LeaveEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal LeaveEvent (ControlPoint controlpoint, Vehicle vehicle)
            {
                ControlPoint = controlpoint;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                throw new NotImplementedException();
            }
            public override string ToString()
            {
                return base.ToString();
            }
        }

        #endregion

        #region Input Events - Getters
        //public Event Input(TLoad load) { return new InternalEvent { This = this, Load = load }; }
        public Event Reach(Vehicle vehicle) { return new ReachEvent(this, vehicle); }
        public Event Leave(Vehicle vehicle) { return new LeaveEvent(this, vehicle); }
        public Event TravelFrom(Vehicle vehicle) { return new TravelFromEvent(this, vehicle); }
        public Event TravelTo(Vehicle vehicle) { return new TravelToEvent(this, vehicle); }
        #endregion

        #region Output Events - Reference to Getters
        //public List<Func<TLoad, Event>> OnOutput { get; private set; } = new List<Func<TLoad, Event>>();
        public List<Func<Event>> OnRelease { get; private set; } = new List<Func<Event>>();
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
        public class ZeroVacancyException : Exception
        {
            public ZeroVacancyException() : base("Control Point is not accessible due to vacancy issue.") { }
        }
        #endregion

        public ControlPoint(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "ControlPoint";
            At = null;
            IncomingSegments = new List<Segment>();
            OutgoingSegments = new List<Segment>();
            HourCounter = new HourCounter();

        }

        public override void WarmedUp(DateTime clockTime)
        {
            throw new NotImplementedException();
        }
    }
}
