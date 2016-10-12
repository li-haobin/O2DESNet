using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;
using MathNet.Numerics.LinearAlgebra.Double;

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
            public List<DenseVector> Coordinates { get; private set; }
            public bool Crab { get; set; } = false;

            public Statics(PathMover.Statics pathMover, double length, double fullSpeed, Direction direction)
            {
                PathMover = pathMover;
                Index = PathMover.Paths.Count;
                Length = length;
                FullSpeed = fullSpeed;
                Direction = direction;
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

            //public virtual void Draw(Graphics g, DrawingParams dParams, Pen pen, double start, double end)
            //{
            //    g.DrawLines(pen, LinearTool.GetCoordsInRange(Coordinates, start, end).Select(c => dParams.GetPoint(c)).ToArray());
            //}
        }
        public enum Direction { Forward, Backward, TwoWay }
        #endregion

        #region Sub-Components
        //internal Server<TScenario, TStatus, TLoad> H_Server { get; private set; }
        //internal Server<TScenario, TStatus, TLoad> R_Server { get; private set; }
        #endregion

        #region Dynamics
        //public HashSet<TLoad> Serving { get { return H_Server.Serving; } }
        //public List<TLoad> Served { get { return H_Server.Served; } }
        //public HashSet<TLoad> Restoring { get { return R_Server.Serving; } }
        //public int Vancancy { get { return Statics.Capacity - Serving.Count - Served.Count - Restoring.Count; } }
        //public int NCompleted { get { return (int)H_Server.HourCounter.TotalDecrementCount; } }     
        #endregion

        #region Events
        //private class RestoreEvent : Event
        //{
        //    public RestoreServer<TLoad> RestoreServer { get; private set; }
        //    public TLoad Load { get; private set; }
        //    internal RestoreEvent(RestoreServer<TLoad> restoreServer, TLoad load)
        //    {
        //        RestoreServer = restoreServer;
        //        Load = load;
        //    }
        //    public override void Invoke()
        //    {
        //        Load.Log(this);
        //        foreach (var evnt in RestoreServer.OnRestore) Execute(evnt());
        //    }
        //    public override string ToString() { return string.Format("{0}_Restore", RestoreServer); }
        //}
        #endregion

        #region Input Events - Getters
        //public Event Depart() { return H_Server.Depart(); }
        //public Event Start(TLoad load)
        //{
        //    if (Vancancy < 1) throw new HasZeroVacancyException();
        //    if (Statics.HandlingTime == null) throw new HandlingTimeNotSpecifiedException();
        //    if (Statics.RestoringTime == null) throw new RestoringTimeNotSpecifiedException();
        //    if (Statics.ToDepart == null) throw new DepartConditionNotSpecifiedException();
        //    return H_Server.Start(load);
        //}
        //public Event Depart() { return new DepartEvent(this); }
        #endregion

        #region Output Events - Reference to Getters
        //public List<Func<TLoad, Event>> OnDepart { get { return H_Server.OnDepart; } }
        //public List<Func<Event>> OnRestore { get; private set; }
        #endregion

        #region Exeptions
        //public class HasZeroVacancyException : Exception
        //{
        //    public HasZeroVacancyException() : base("Check vacancy of the Server before execute Start event.") { }
        //}
        //public class HandlingTimeNotSpecifiedException : Exception
        //{
        //    public HandlingTimeNotSpecifiedException() : base("Set HandlingTime as a random generator.") { }
        //}
        //public class RestoringTimeNotSpecifiedException : Exception
        //{
        //    public RestoringTimeNotSpecifiedException() : base("Set RestoringTime as a random generator.") { }
        //}
        //public class DepartConditionNotSpecifiedException : Exception
        //{
        //    public DepartConditionNotSpecifiedException() : base("Set ToDepart as depart condition.") { }
        //}
        #endregion

        public Path(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Path";

            //H_Server = new Server<TLoad>(statics.H_Server, DefaultRS.Next());
            //R_Server = new Server<TLoad>(statics.R_Server, DefaultRS.Next());

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
