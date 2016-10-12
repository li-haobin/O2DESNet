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
            public Direction Direction { get; private set; }
            public double FullSpeed { get; private set; }
            public List<ControlPoint.Statics> ControlPoints { get; private set; }
            public int Capacity { get; private set; }

            public List<DenseVector> Coordinates { get; private set; }
            public bool Crab { get; set; } = false;

            public Statics(PathMover.Statics pathMover, double length, double fullSpeed, Direction direction, int capacity = 1)
            {
                PathMover = pathMover;
                Index = PathMover.Paths.Count;
                Length = length;
                FullSpeed = fullSpeed;
                Direction = direction;
                Capacity = capacity;

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
        //internal Server<TScenario, TStatus, TLoad> H_Server { get; private set; }
        //internal Server<TScenario, TStatus, TLoad> R_Server { get; private set; }
        #endregion

        #region Dynamics
        public PathMover PathMover { get; internal set; }
        public Dictionary<Vehicle, Tuple<ControlPoint, ControlPoint>> ActiveSegments { get; private set; }  
        public int Vacancy { get { return Config.Capacity - ActiveSegments.Count; } }
        public List<Vehicle> WaitingToExit { get; private set; }
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
                if (Path.ActiveSegments.ContainsKey(Vehicle)) throw new StatusException("Vehicle already exists on the path.");
                Path.ActiveSegments.Add(Vehicle, new Tuple<ControlPoint, ControlPoint>(Vehicle.Current, null));
                Execute(new MoveEvent(Path, Vehicle));
            }
            public override string ToString() { return string.Format("{0}_Enter", Path); }
        }
        private class ExitEvent : Event
        {
            public Path Path { get; private set; }
            internal ExitEvent(Path path)
            {
                Path = path;
            }
            public override void Invoke()
            {
                //if (Vehicle.PathToNext.Vacancy > 0)
                //{
                //    Vehicle.Log(this);
                //    Path.ActiveSegments.Remove(Vehicle);
                //    foreach (var evnt in Path.OnExit) Execute(evnt(Vehicle));
                //}
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
                if (!Vehicle.PathToNext.Equals(Path))
                {
                    Path.WaitingToExit.Add(Vehicle);
                    Execute(new ExitEvent(Path));
                }
                else
                {
                    Schedule(new ReachEvent(Path, Vehicle), TimeSpan.FromSeconds(Vehicle.Current.Config.GetDistanceTo(Vehicle.Next.Config) / Vehicle.Category.Speed));
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
                foreach (var evnt in Path.OnExit) Execute(evnt(Vehicle));
            }
            public override string ToString() { return string.Format("{0}_Reach", Path); }
        }
        #endregion

        #region Input Events - Getters
        public Event Enter(Vehicle vehicle) { return new EnterEvent(this, vehicle); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Vehicle, Event>> OnExit { get; private set; }
        #endregion

        #region Exeptions
        public class StatusException : Exception
        {
            public StatusException(string msg) : base(string.Format("Path Status Exception: {0}", msg)) { }
        }
        #endregion

        public Path(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Path";
            ActiveSegments = new Dictionary<Vehicle, Tuple<ControlPoint, ControlPoint>>();
            WaitingToExit = new List<Vehicle>();

            // connect sub-components
            //H_Server.OnDepart.Add(R_Server.Start());
            //R_Server.Statics.ToDepart = load => true;
            //R_Server.OnDepart.Add(l => new RestoreEvent(this, l));

            // initialize for output events
            //OnRestore = new List<Func<Event>>(); 

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
