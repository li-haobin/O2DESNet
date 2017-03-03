using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace Test
{
    public class Segment : Component<Segment.Statics>
    {
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

            public double Length { get; set; }
            public Direction Direction { get; set; }
            public double FullSpeed { get; set; }
            public int Capacity { get; set; }
            public List<ControlPoint.Statics> ControlPoints { get; private set; }
            /// <summary>
            /// Do not change the direction of the vehicle
            /// </summary>
            public bool Crab { get; set; } = false;

            internal HashSet<Statics> Conflicts { get; private set; } = new HashSet<Statics>();
            public void AddConflict(Statics path) { }

            public Statics(PathMover.Statics pathMover)
            {
                PathMover = pathMover;
                Index = PathMover.Segments.Count;
                Capacity = int.MaxValue;
                ControlPoints = new List<ControlPoint.Statics>();
            }

            internal void Add(ControlPoint.Statics controlPoint, double position) { }
            public double GetDistance(ControlPoint.Statics from, ControlPoint.Statics to) { return 0; }          
        }
        public enum Direction { Forward, Backward, TwoWay }
        //public new Statics Config { get { return (Statics)base.Config; } } // for inheritated component       
        #endregion

        #region Sub-Components
        //private Server<TLoad> Server { get; set; }

        #endregion

        #region Dynamics
        //public int Occupancy { get { return Server.Occupancy; } }  
        
        public PathMover PathMover { get; internal set; }
        public ControlPoint[] ControlPoints { get; } //have not defined yet
        public int Vacancy { get; }//have not defined yet

        /// <summary>
        /// Check if there is any conflicting path has zero vacancy
        /// </summary>
        /// <param name="via">the path where the vehicle is on</param>
        public bool HasConflict(Segment via = null) { return false; }//not completed
        public double Utilization { get; }//not completed
        #endregion

        #region Events
        private abstract class EventOfSegment : Event { internal Segment This { get; set; } } // event adapter 
                                                                                              //private class InternalEvent : EventOfSegment
                                                                                              //{
                                                                                              //    internal TLoad Load { get; set; }
                                                                                              //    public override void Invoke() {  }
                                                                                              //}
        private class ExitEvent : Event
        {
            public Segment Segment { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal ExitEvent(Segment segment, Vehicle vehicle)
            {
                Segment = segment;
                Vehicle = vehicle;
            }
            public override void Invoke() { }
        }
        private class MoveEvent : Event
        {
            public Segment Segment { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal MoveEvent(Segment segment, Vehicle vehicle)
            {
                Segment = segment;
                Vehicle = vehicle;
            }
            public override void Invoke() { }
        }
        private class ReachEvent : Event
        {
            public Segment Segment { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal ReachEvent(Segment segment, Vehicle vehicle)
            {
                Segment = segment;
                Vehicle = vehicle;
            }
            public override void Invoke() { }
        }
        #endregion

        #region Input Events - Getters
        //public Event Input(TLoad load) { return new InternalEvent { This = this, Load = load }; }
        internal Event Move(Vehicle vehicle) { return new MoveEvent(this, vehicle); }
        #endregion

        #region Output Events - Reference to Getters
        //public List<Func<TLoad, Event>> OnOutput { get; private set; } = new List<Func<TLoad, Event>>();
        #endregion

        #region Exeptions
        public class StatusException : Exception
        {
            public StatusException(string msg) : base(string.Format("Path Status Exception: {0}", msg)) { }
        }
        public class ToExitConditionNotSpecifiedException : Exception
        {
            public Segment Path { get; set; }
            public ToExitConditionNotSpecifiedException() : base("ToExit condition of Path has to be specified.") { }
        }
        public class HasZeroVacancy : Exception
        {
            public Segment Path { get; set; }
            public Vehicle Vehicle { get; set; }
            public HasZeroVacancy() : base("Ensure Path has positive vacancy before move Vehicle in.") { }
        }
        public class CollisionException : Exception
        {
            public ControlPoint ControlPoint1 { get; private set; }
            public ControlPoint ControlPoint2 { get; private set; }
            public Vehicle Vehicle1 { get; private set; }
            public Vehicle Vehicle2 { get; private set; }
            public Segment Path { get; private set; }
            public DateTime ClockTime { get; private set; }
            public CollisionException(Segment path, ControlPoint cp1, ControlPoint cp2, Vehicle veh1, Vehicle veh2, DateTime clockTime) :
                base(string.Format("{5} - Collision identified on {0} between {1} & {2} by {3} & {4}.", path, cp1, cp2, veh1, veh2, clockTime))
            {
                Path = path;
                ControlPoint1 = cp1; ControlPoint2 = cp2;
                Vehicle1 = veh1; Vehicle2 = veh2;
                ClockTime = clockTime;
            }
        }
        #endregion

        public Segment(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Segment";
        }

        public override void WarmedUp(DateTime clockTime)
        {
            throw new NotImplementedException();
        }
    }
}
