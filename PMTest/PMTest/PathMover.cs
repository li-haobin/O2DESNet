using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace Test
{
    public class PathMover : Component<PathMover.Statics>
    {
        #region Statics

        
        public class Statics : Scenario
        {
            private static int _count = 0;
            public int Index { get; private set; } = _count++;
            public List<Segment.Statics> Segments { get; private set; }
            public List<ControlPoint.Statics> ControlPoints { get; private set; }            

            public Statics()
            {
                Segments = new List<Segment.Statics>();
                ControlPoints = new List<ControlPoint.Statics>();
            }

            #region Path Mover Builder
            /// <summary>
            /// Create and return a new path
            /// </summary>
            public Segment.Statics CreateSegment(double length, double fullSpeed, int capacity = int.MaxValue, Segment.Direction direction = Segment.Direction.TwoWay)
            {
                CheckInitialized();
                var segment = new Segment.Statics(this)
                {
                    Length = length,
                    FullSpeed = fullSpeed,
                    Capacity = capacity,
                    Direction = direction,
                };
                Segments.Add(segment);
                return segment;
            }

            /// <summary>
            /// Create and return a new control point
            /// </summary>
            public ControlPoint.Statics CreateControlPoint(Segment.Statics segment, double position)
            {
                CheckInitialized();
                var controlPoint = new ControlPoint.Statics(this);
                segment.Add(controlPoint, position);
                ControlPoints.Add(controlPoint);
                return controlPoint;
            }

            /// <summary>
            /// Connect two paths at specified positions
            /// </summary>
            public void Connect(Segment.Statics segment_0, Segment.Statics segment_1, double position_0, double position_1)
            {
                CheckInitialized();
                segment_1.Add(CreateControlPoint(segment_0, position_0), position_1);
            }

            /// <summary>
            /// Connect the Path to the Control Point at specific positions
            /// </summary>
            public void Connect(Segment.Statics segment, double position, ControlPoint.Statics controlPoint)
            {
                CheckInitialized();
                if (controlPoint.Positions.ContainsKey(segment)) throw new StaticsBuildException("The Control Point exists on the Path.");
                segment.Add(controlPoint, position);
            }

            /// <summary>
            /// Connect the end of path_0 to the start of path_1
            /// </summary>
            public void Connect(Segment.Statics segment_0, Segment.Statics segment_1) { Connect(segment_0, segment_1, segment_0.Length, 0); }

            private void CheckInitialized()
            {
                if (_initialized) throw new StaticsBuildException("PathMover cannot be modified after initialization.");//_initilized defined in the region for Static Routing(distance based), add in?
            }
            #endregion

            
        }
        //public new Statics Config { get { return (Statics)base.Config; } } // for inheritated component
        #endregion

        #region Sub-Components
        //private Server<TLoad> Server { get; set; }
        #endregion

        #region Dynamics
        //public int Occupancy { get { return Server.Occupancy; } }  
        public Dictionary<ControlPoint.Statics, ControlPoint> ControlPoints { get; private set; }
        public Dictionary<Segment.Statics, Segment> Paths { get; private set; }
        public HashSet<Vehicle> Vehicles { get; private set; }
        public HashSet<Vehicle> VehiclesHistory { get; private set; } = new HashSet<Vehicle>();
        public void RecordVehiclePostures(DateTime clockTime)
        {
            //foreach (var veh in Vehicles)
            //veh.Postures.Add(new Tuple<DateTime, Tuple<Point, double>>(clockTime, veh.GetPosture(clockTime)));
        }
        public DateTime StartTime { get; private set; } = DateTime.MinValue;
        public DateTime LastUpdateTime { get; internal set; }
        #endregion

        #region Events
        private abstract class EventOfPathMover : Event { internal PathMover This { get; set; } } // event adapter 
                                                                                                  //private class InternalEvent : EventOfPathMover
                                                                                                  //{
                                                                                                  //    internal TLoad Load { get; set; }
                                                                                                  //    public override void Invoke() {  }
                                                                                                  //}
        #endregion

        #region Input Events - Getters
        //public Event Input(TLoad load) { return new InternalEvent { This = this, Load = load }; }
        #endregion

        #region Output Events - Reference to Getters
        //public List<Func<TLoad, Event>> OnOutput { get; private set; } = new List<Func<TLoad, Event>>();
        #endregion

        #region Exeptions
        public class StaticsBuildException : Exception
        {
            public StaticsBuildException(string msg) : base(string.Format("PathMover Build Exception: {0}", msg)) { }
        }
        #endregion

        public PathMover(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "PathMover";

        }

        public override void WarmedUp(DateTime clockTime)
        {
            throw new NotImplementedException();
        }

        public override void WriteToConsole(DateTime? clockTime) { }
    }
}
