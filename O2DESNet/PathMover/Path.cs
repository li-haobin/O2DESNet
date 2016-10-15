using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Drawing;
using O2DESNet.Optimizer;

namespace O2DESNet
{
    public class Path : Component<Path.Statics>
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

            public List<DenseVector> Coordinates { get; private set; }
            public bool Crab { get; set; } = false;

            public Statics(PathMover.Statics pathMover)
            {
                PathMover = pathMover;
                Index = PathMover.Paths.Count;
                Capacity = int.MaxValue;

                ControlPoints = new List<ControlPoint.Statics>();
                Coordinates = new List<DenseVector>();
            }

            internal void Add(ControlPoint.Statics controlPoint, double position)
            {
                controlPoint.Positions.Add(this, position);
                ControlPoints.Add(controlPoint);
                if (controlPoint.Positions[this] < 0 || controlPoint.Positions[this] > Length)
                    throw new Exception("Control point must be positioned within the range of path length.");
                ControlPoints.Sort((t0, t1) => t0.Positions[this].CompareTo(t1.Positions[this]));
            }
            public double GetDistance(ControlPoint.Statics from, ControlPoint.Statics to)
            {
                if (from.Positions.ContainsKey(this) && to.Positions.ContainsKey(this))
                {
                    double distance = to.Positions[this] - from.Positions[this];
                    if (Direction == Direction.Backward) distance = -distance;
                    else if (Direction == Direction.TwoWay) distance = Math.Abs(distance);
                    if (distance >= 0) return distance;
                }
                return double.PositiveInfinity;
            }

            public virtual void Draw(Graphics g, DrawingParams dParams, Pen pen, double start, double end)
            {
                g.DrawLines(pen, LinearTool.GetCoordsInRange(Coordinates, start, end).Select(c => dParams.GetPoint(c)).ToArray());
            }
        }
        public enum Direction { Forward, Backward, TwoWay }
        #endregion

        #region Sub-Components
        internal FIFOServer<Vehicle>[] ForwardSegments { get; private set; }         
        internal FIFOServer<Vehicle>[] BackwardSegments { get; private set; }
        /// <summary>
        /// The routes on the path to be followed by the vehicle
        /// </summary>
        private Dictionary<Vehicle, List<ControlPoint>> Routes { get; set; }
        /// <summary>
        /// Get index of given control point
        /// </summary>
        internal Dictionary<ControlPoint, int> Indices
        {
            get
            {
                if (_indices == null) _indices = Enumerable.Range(0, Config.ControlPoints.Count)
                        .ToDictionary(i => PathMover.ControlPoints[Config.ControlPoints[i]], i => i);
                return _indices;
            }
        }
        private Dictionary<ControlPoint, int> _indices = null;
        /// <summary>
        /// Distances between adjacent control point
        /// </summary>
        private double[] AbsDistances { get; set; }
        #endregion

        #region Dynamics
        public PathMover PathMover { get; internal set; }
        public ControlPoint[] ControlPoints { get { return Config.ControlPoints.Select(cp => PathMover.ControlPoints[cp]).ToArray(); } }
        public int Vacancy { get { return Config.Capacity - ForwardSegments.Concat(BackwardSegments).Sum(seg => seg.NOccupied) - ControlPoints.Count(cp => cp.At != null); } }
        /// <summary>
        /// Vehicles that currently travelling on the Path.
        /// </summary>
        public HashSet<Vehicle> Travelling { get { return new HashSet<Vehicle>(ForwardSegments.Concat(BackwardSegments).SelectMany(seg => seg.Serving).OrderBy(veh => veh.Id)); } }
        /// <summary>
        /// Vehicles travelling on the Path, which are delayed due to slow moving ahead. 
        /// </summary>
        public HashSet<Vehicle> Delayed { get { return new HashSet<Vehicle>(ForwardSegments.Concat(BackwardSegments).SelectMany(seg => seg.Delayed).OrderBy(veh => veh.Id)); } }
        /// <summary>
        /// Vehicles stopped on the Path.
        /// </summary>
        public HashSet<Vehicle> Stopped
        {
            get
            {
                var stopped = new List<Vehicle>(ForwardSegments.Concat(BackwardSegments).SelectMany(seg => seg.Served)); // stopped inside segments due to congestion
                foreach (var cp in ControlPoints) if (cp.At != null) stopped.Add(cp.At); // stopped at Control Points
                return new HashSet<Vehicle>(stopped.OrderBy(veh => veh.Id));
            }
        }
        /// <summary>
        /// All vehicles occupying the Path.
        /// </summary>
        public HashSet<Vehicle> Occupying { get { return new HashSet<Vehicle>(Travelling.Concat(Stopped).OrderBy(veh => veh.Id)); } }
        #endregion

        #region Events
        private class EnterEvent : Event
        {
            public Path Path { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal EnterEvent(Path path, Vehicle vehicle)
            {
                Path = path;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                Vehicle.Log(this);
                if (!Path.Indices.ContainsKey(Vehicle.Current)) throw new StatusException("Vehicle has to be moved to any Control Point on the path before enter it.");
                if (Path.Routes.ContainsKey(Vehicle)) throw new StatusException("Vehicle already exists on the path.");
                
                Path.Routes.Add(Vehicle, new List<ControlPoint>());
                var cp = Vehicle.Current.Config;
                while (true)
                {
                    if (cp.Equals(Vehicle.Target.Config)) break;
                    var next = cp.RoutingTable[Vehicle.Target.Config];
                    var path = cp.PathingTable[next];
                    if (!path.Equals(Path.Config))
                    {
                        break;
                    }
                    else
                    {
                        Path.Routes[Vehicle].Add(Path.PathMover.ControlPoints[next]);
                        cp = next;
                    }
                } 
                Execute(new MoveEvent(Path, Vehicle));
            }
            public override string ToString() { return string.Format("{0}_Enter", Path); }
        }
        private class ExitEvent : Event
        {
            public Path Path { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal ExitEvent(Path path, Vehicle vehicle)
            {
                Path = path;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                Vehicle.Log(this);
                Path.Routes.Remove(Vehicle);
                Execute(new EnterEvent(Vehicle.PathToNext, Vehicle));

                // pull vehicles from other paths to enter when vacancy is released
                foreach (var seg in Path.ControlPoints // all Control Points on the Path
                    .SelectMany(cp => cp.IncomingSegments.Where(seg => seg.Served.Count > 0)) // their incoming Segements where have vehicles stucked
                    .OrderBy(seg => seg.FinishTime.Value)) // order by finish time
                    Execute(seg.Depart());
            }
            public override string ToString() { return string.Format("{0}_Exit", Path); }
        }
        private class MoveEvent : Event
        {
            public Path Path { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal MoveEvent(Path path, Vehicle vehicle)
            {
                Path = path;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                Vehicle.Log(this);
                var startIndex = Path.Indices[Vehicle.Current];
                var endIndex = Path.Indices[Path.Routes[Vehicle].First()];
                if (startIndex < endIndex)
                {
                    if (Path.BackwardSegments[startIndex].NOccupied > 0)
                        throw new CollisionException(Path, Vehicle.Current, Path.Routes[Vehicle].First(),
                            Vehicle, Path.BackwardSegments[startIndex].Sequence.First(), ClockTime);
                    Execute(Vehicle.Move(Path.Routes[Vehicle].First()));
                    Execute(Path.ForwardSegments[startIndex].Start(Vehicle));
                }
                else
                {
                    if (Path.ForwardSegments[endIndex].NOccupied > 0)
                        throw new CollisionException(Path, Vehicle.Current, Path.Routes[Vehicle].First(),
                            Vehicle, Path.ForwardSegments[endIndex].Sequence.First(), ClockTime);
                    Execute(Path.BackwardSegments[endIndex].Start(Vehicle));
                }
            }
            public override string ToString() { return string.Format("{0}_Move", Path); }
        }
        private class ReachEvent : Event
        {
            public Path Path { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal ReachEvent(Path path, Vehicle vehicle)
            {
                Path = path;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                Vehicle.Log(this);
                Execute(Vehicle.Reach(Path.Routes[Vehicle].First()));
                Path.Routes[Vehicle].RemoveAt(0);

                if (Path.Routes[Vehicle].Count > 0) Execute(new MoveEvent(Path, Vehicle)); // keep moving on the Path
                else if (Vehicle.PathToNext != null) Execute(new ExitEvent(Path, Vehicle));
            }
            public override string ToString() { return string.Format("{0}_Reach", Path); }
        }
        #endregion

        #region Input Events - Getters
        internal Event Enter(Vehicle vehicle) { return new EnterEvent(this, vehicle); }
        #endregion

        #region Output Events - Reference to Getters
        #endregion

        #region Exeptions
        public class StatusException : Exception
        {
            public StatusException(string msg) : base(string.Format("Path Status Exception: {0}", msg)) { }
        }
        public class ToExitConditionNotSpecifiedException : Exception
        {
            public Path Path { get; set; }
            public ToExitConditionNotSpecifiedException() : base("ToExit condition of Path has to be specified.") { }
        }
        public class HasZeroVacancy : Exception
        {
            public Path Path { get; set; }
            public Vehicle Vehicle { get; set; }
            public HasZeroVacancy() : base("Ensure Path has positive vacancy before move Vehicle in.") { }
        }
        public class CollisionException : Exception
        {
            public ControlPoint ControlPoint1 { get; private set; }
            public ControlPoint ControlPoint2 { get; private set; }
            public Vehicle Vehicle1 { get; private set; }
            public Vehicle Vehicle2 { get; private set; }
            public Path Path { get; private set; }
            public DateTime ClockTime { get; private set; }
            public CollisionException(Path path, ControlPoint cp1, ControlPoint cp2, Vehicle veh1, Vehicle veh2, DateTime clockTime) :
                base(string.Format("{5} - Collision identified on {0} between {1} & {2} by {3} & {4}.", path, cp1, cp2, veh1, veh2, clockTime))
            {
                Path = path;
                ControlPoint1 = cp1; ControlPoint2 = cp2;
                Vehicle1 = veh1; Vehicle2 = veh2;
                ClockTime = clockTime;
            }
        }
        #endregion

        public Path(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Path";

            // initialize dynamic properties
            int n = config.ControlPoints.Count;
            
            AbsDistances = Enumerable.Range(0, n - 1).Select(i => Math.Abs(Config.GetDistance(Config.ControlPoints[i], Config.ControlPoints[i + 1]))).ToArray();
            Func<int, FIFOServer<Vehicle>> getSegment = i =>
            {
                var segment = new FIFOServer<Vehicle>(
                    new FIFOServer<Vehicle>.Statics
                    {
                        Capacity = Config.Capacity,
                        ServiceTime = (veh, rs) => TimeSpan.FromSeconds(AbsDistances[i] / Math.Min(Config.FullSpeed, veh.Speed)),
                        ToDepart = veh => Routes[veh].First().Accessible(via: this)
                    },
                    DefaultRS.Next());
                segment.OnDepart.Add(veh => new ReachEvent(this, veh));
                return segment;
            };
            ForwardSegments = Enumerable.Range(0, n - 1).Select(i => getSegment(i)).ToArray();
            BackwardSegments = Enumerable.Range(0, n - 1).Select(i => getSegment(i)).ToArray();
            Routes = new Dictionary<Vehicle, List<ControlPoint>>();
            

            // connect sub-components
            //H_Server.OnDepart.Add(R_Server.Start());
            //R_Server.Statics.ToDepart = load => true;
            //R_Server.OnDepart.Add(l => new RestoreEvent(this, l));

            // initialize for output events

            // initialize event, compulsory if it's assembly
            //InitEvents.Add(R_Server.Start());
        }

        public override void WarmedUp(DateTime clockTime)
        {
            //H_Server.WarmedUp(clockTime);
            //R_Server.WarmedUp(clockTime);
        }

        public override void WriteToConsole()
        {
            //Console.WriteLine("[{0}]", this);
            //Console.Write("Serving: ");
            //foreach (var load in Serving) Console.Write("{0} ", load);
            //Console.WriteLine();
            //Console.Write("Served: ");
            //foreach (var load in Served) Console.Write("{0} ", load);
            //Console.WriteLine();
            //Console.Write("Restoring: ");
            //foreach (var load in Restoring) Console.Write("{0} ", load);
            //Console.WriteLine();
        }
    }
}
