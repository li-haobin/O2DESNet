using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Drawing;
using O2DESNet.Optimizer;
using O2DESNet.SVGRenderer;
using System.Xml.Linq;

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

            /// <summary>
            /// For old path mover drawing
            /// </summary>
            public List<DenseVector> Coordinates { get; private set; }

            /// <summary>
            /// For path mover graphical display
            /// </summary>
            public List<Point> Coords { get; set; }
            
            /// <summary>
            /// Do not change the direction of the vehicle
            /// </summary>
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

            #region SVG Output
            /// <summary>
            /// SVG Description
            /// </summary>
            public string Description { get; set; }

            public Group SVG()
            {
                string name = "path#" + Index;
                var g = new Group(name, new O2DESNet.SVGRenderer.Path(LineStyle, Description, new XAttribute("id", name + "_d")));
                var label = new Text(LabelStyle, string.Format("PATH{0}", Index), new XAttribute("transform", "translate(-10 -7)"));
                if (Direction != Direction.Backward) g.Add(new PathMarker(name + "_marker", name + "_d", 0.333, new Use("arrow"), label)); // forwards & bi-directional
                else g.Add(new PathMarker(name + "_marker", name + "_d", 0.667, new Use("arrow", 0, 0, 180), label)); // backwards
                if (Direction == Direction.TwoWay) g.Add(new PathMarker(name + "_marker2", name + "_d", 0.667, new Use("arrow", 0, 0, 180))); // bi-directional
                return g;
            }

            public static CSS LineStyle = new CSS("pm_path", new XAttribute("stroke", "black"), new XAttribute("stroke-dasharray", "3,3"), new XAttribute("fill", "none"));
            public static CSS LabelStyle = new CSS("pm_path_label", new XAttribute("text-anchor", "start"), new XAttribute("font-family", "Verdana"), new XAttribute("font-size", "9px"), new XAttribute("fill", "black"));

            /// <summary>
            /// Including arrows, styles
            /// </summary>
            public static Definition SVGDefs
            {
                get
                {
                    return new Definition(
                        new SVGRenderer.Path("M -10 -4 L 0 0 L -10 4", "black", new XAttribute("id", "arrow")),
                        new Style(LineStyle, LabelStyle)
                        );
                }
            }
            #endregion
        }
        public enum Direction { Forward, Backward, TwoWay }
        #endregion

        #region Sub-Components
        public class Segment : FIFOServer<Vehicle>
        {
            public Path Path { get; private set; }
            /// <summary>
            /// The ratio of the start on the path.
            /// </summary>
            public double StartRatio { get; private set; }
            /// <summary>
            /// The ratio of the end on the path.
            /// </summary>
            public double EndRatio { get; private set; }
            public ControlPoint.Statics StartPoint { get; private set; }
            public ControlPoint.Statics EndPoint { get; private set; }
            public bool Forward { get; private set; }

            public Segment(Path path, ControlPoint.Statics startPoint, ControlPoint.Statics endPoint, Statics config, int seed, string tag = null)
                : base(config, seed, tag)
            {
                Path = path;
                StartPoint = startPoint; EndPoint = endPoint;
                StartRatio = StartPoint.Positions[Path.Config] / Path.Config.Length;
                EndRatio = EndPoint.Positions[Path.Config] / Path.Config.Length;
                Forward = StartRatio <= EndRatio;
            }

            public void LogAnchors(Vehicle vehicleOnDepart, DateTime clockTime)
            {
                if (vehicleOnDepart == null) vehicleOnDepart = Path.PathMover.ControlPoints[EndPoint].At;
                // postures of vehicles on the Segment
                var time = (clockTime - Path.PathMover.StartTime).TotalSeconds;
                var furthest = Path.Config.Length * (EndRatio - StartRatio) - 
                    (vehicleOnDepart != null ? vehicleOnDepart.Category.SafetyLength / 2 : 0);
                foreach (var veh in Sequence)
                {
                    var distance = Math.Min(
                        furthest,
                        (clockTime - StartTimes[veh]).TotalSeconds * // time lapsed since enter
                        Math.Min(veh.Category.Speed, Path.Config.FullSpeed) // speed taken
                        );
                    furthest = distance - veh.Category.SafetyLength;

                    veh.LogAnchor(time, Path.Config, StartRatio + Math.Max(0, (distance - veh.Category.SafetyLength / 2)) / Path.Config.Length * (Forward ? 1 : -1));
                }
            }
        }

        internal Segment[] ForwardSegments { get; private set; }         
        internal Segment[] BackwardSegments { get; private set; }
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
        public double Utilization
        {
            get
            {
                return (ForwardSegments.Concat(BackwardSegments).Sum(seg => seg.HourCounter.CumValue) +
                    ControlPoints.Sum(cp => cp.HourCounter.CumValue)) /
                    ForwardSegments.First().HourCounter.TotalHours / Config.Capacity;
            }
        }
        #endregion

        #region Events
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
                
                /*************** PULL WHEN VEHICLE EXIT A PATH ****************/
                // path vacancy is released
                foreach (var seg in Path.ControlPoints // all Control Points on the Path
                    .SelectMany(cp => cp.IncomingSegments.Where(seg => seg.ReadyTime != null)) // segment ready for vehicle to depart
                    .OrderBy(seg => seg.ReadyTime.Value)) // order by finish time
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
                var seg = Vehicle.Segment; // record previous segment 
                var next = Vehicle.Next;
                var startIndex = Path.Indices[Vehicle.Current];
                var endIndex = Path.Indices[next];
                if (startIndex < endIndex)
                {
                    if (Path.BackwardSegments[startIndex].NOccupied > 0)
                        throw new CollisionException(Path, Vehicle.Current, next,
                            Vehicle, Path.BackwardSegments[startIndex].Sequence.First(), ClockTime);
                    Execute(Vehicle.Move());
                    Execute(Path.ForwardSegments[startIndex].Start(Vehicle));
                    Vehicle.Segment = Path.ForwardSegments[startIndex];
                }
                else
                {
                    if (Path.ForwardSegments[endIndex].NOccupied > 0)
                        throw new CollisionException(Path, Vehicle.Current, next,
                            Vehicle, Path.ForwardSegments[endIndex].Sequence.First(), ClockTime);
                    Execute(Path.BackwardSegments[endIndex].Start(Vehicle));
                    Vehicle.Segment = Path.BackwardSegments[endIndex];
                }

                // record posture of vehicles
                var time = (ClockTime - Path.PathMover.StartTime).TotalSeconds;
                if (seg != null)
                {
                    Vehicle.LogAnchor(time, seg.Path.Config, seg.EndRatio);
                    seg.LogAnchors(Vehicle, ClockTime);
                }
                if (Vehicle.Segment != null) Vehicle.LogAnchor(time, Vehicle.Segment.Path.Config, Vehicle.Segment.StartRatio);
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
                Execute(Vehicle.Reach());
                Path.PathMover.LastUpdateTime = ClockTime;

                var seg = Vehicle.Segment;
                var pathToNext = Vehicle.PathToNext;                
                var time = (ClockTime - Path.PathMover.StartTime).TotalSeconds;
                if (seg != null)
                {
                    Vehicle.LogAnchor(time, seg.Path.Config, seg.EndRatio);
                    seg.LogAnchors(Vehicle, ClockTime);
                }

                if (Vehicle.PathToNext != null) Execute(new MoveEvent(pathToNext, Vehicle));
                if (pathToNext != Path) Execute(new ExitEvent(Path, Vehicle));
                
                if (Vehicle.Targets.Count == 0) Execute(Vehicle.Complete());               
            }
            public override string ToString() { return string.Format("{0}_Reach", Path); }
        }
        #endregion

        #region Input Events - Getters
        internal Event Move(Vehicle vehicle) { return new MoveEvent(this, vehicle); }
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
            var ratios = Enumerable.Range(0, n).Select(i => Config.ControlPoints[i].Positions[Config] / Config.Length).ToArray();
            Func<int, bool, Segment> getSegment = (i, forward) =>
            {
                var segment = new Segment(
                    this, forward ? Config.ControlPoints[i] : Config.ControlPoints[i + 1], forward ? Config.ControlPoints[i + 1] : Config.ControlPoints[i],
                    new FIFOServer<Vehicle>.Statics
                    {
                        Capacity = Config.Capacity,
                        ServiceTime = (veh, rs) => TimeSpan.FromSeconds(AbsDistances[i] / Math.Min(Config.FullSpeed, veh.Speed)),
                        ToDepart = veh => veh.Next.Accessible(via: this),
                        MinInterDepartureTime = (veh1, veh2, rs) => TimeSpan.FromSeconds(
                            (veh1.Category.SafetyLength + veh2.Category.SafetyLength) / 2 // distance between centers of the two consecutive vehicles, i.e., the gap
                            / new double[] { Config.FullSpeed, veh1.Speed, veh2.Speed }.Min() // the speed to fill the gap
                            ),
                    },
                    DefaultRS.Next());
                segment.OnDepart.Add(veh => new ReachEvent(this, veh));
                segment.OnDelay.Add(veh => veh.UpdatePositionsInSegment());
                return segment;
            };
            ForwardSegments = Enumerable.Range(0, n - 1).Select(i => getSegment(i, true)).ToArray();
            BackwardSegments = Enumerable.Range(0, n - 1).Select(i => getSegment(i, false)).ToArray();   
        }

        public override void WarmedUp(DateTime clockTime)
        {
            foreach (var seg in ForwardSegments.Concat(BackwardSegments)) seg.WarmedUp(clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime = null)
        {
            Console.Write("{0} (Util.{1:F4}):\t", this, Utilization);
            for (int i = 0; i < ControlPoints.Length - 1; i++)
            {
                ControlPoints[i].WriteToConsole(clockTime);
                Console.Write(" ");

                var forward = ForwardSegments[i].Sequence.ToList(); forward.Reverse();
                var backward = BackwardSegments[i].Sequence.ToList();
                if (forward.Count > 0)
                {
                    Console.Write("- ");
                    foreach (var veh in forward.Concat(backward))
                    {
                        Console.Write(veh);
                        //if (ForwardSegments[i].Delayed.Contains(veh)) Console.Write("!");
                        if (ForwardSegments[i].Served.Contains(veh)) Console.Write("!!");
                        Console.Write(" ");
                    }
                    Console.Write("-> ");
                }
                else if (backward.Count > 0)
                {
                    Console.Write("<- ");
                    foreach (var veh in forward.Concat(backward))
                    {
                        Console.Write(veh);
                        //if (BackwardSegments[i].Delayed.Contains(veh)) Console.Write("!");
                        if (BackwardSegments[i].Served.Contains(veh)) Console.Write("!!");
                        Console.Write(" ");
                    }
                    Console.Write("- ");
                }
                else Console.Write("- ");
            }
            ControlPoints.Last().WriteToConsole();
            Console.WriteLine();
        }
    }
}
