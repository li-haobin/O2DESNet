using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;
using O2DESNet.SVGRenderer;
using System.Xml.Linq;

namespace O2DESNet
{
    public class ControlPoint : Component<ControlPoint.Statics>
    {
        #region Sub-Components
        internal List<Path.Segment> IncomingSegments { get; private set; }
        internal List<Path.Segment> OutgoingSegments { get; private set; }
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
            public Path.Statics GetPathFor(Statics target)
            {
                if (target == this) return null;
                var nextCP = RoutingTable[target];
                if (nextCP == null) return null;
                return PathingTable[nextCP];
            }

            #region SVG Output
            public Group SVG()
            {
                string cp_name = "cp#" + PathMover.Index + "_" + Index;
                var path = Positions.Select(i => i.Key).OrderBy(p => p.Index).First();
                string path_name = "path#" + PathMover.Index + "_" + path.Index;
                var label = new Text(LabelStyle, string.Format("CP{0}", Index), new XAttribute("transform", "translate(3 6)"));
                if (path.X != 0 || path.Y != 0 || path.Rotate != 0)
                    return new PathMarker(cp_name, path.X, path.Y, path.Rotate, path_name + "_d", Positions[path] / path.Length, new Use("cross"), label);
                else return new PathMarker(cp_name, path_name + "_d", Positions[path] / path.Length, new Use("cross"), label);
            }
            
            public static CSS LabelStyle = new CSS("pm_cp_label", new XAttribute("text-anchor", "left"), new XAttribute("font-family", "Verdana"), new XAttribute("font-size", "4px"), new XAttribute("fill", "darkred"));

            /// <summary>
            /// Including arrows, styles
            /// </summary>
            public static Definition SVGDefs
            {
                get
                {
                    return new Definition(
                        new SVGRenderer.Path("M -2 -2 L 2 2 M -2 2 L 2 -2", "darkred", new XAttribute("id", "cross"), new XAttribute("stroke-width", "0.5")),
                        new Style(LabelStyle)
                        );
                }
            }
            #endregion
        }
        #endregion

        #region Dynamics
        public PathMover PathMover { get; internal set; }
        public Vehicle At { get; internal set; }
        public bool Locked { get; private set; } = false;
        /// <summary>
        /// Chech if the control point is accessible via the given path
        /// </summary>
        public bool Accessible (Path via = null)
        {
            //if (At != null) return false;
            if (Locked) return false;
            foreach(var path in Config.Positions.Keys.Select(i => PathMover.Paths[i]))
                if (path != via && path.Vacancy < 1) return false;
            return true;
        }
        public HourCounter HourCounter { get; private set; }
        #endregion

        #region Events
        internal class PutOnEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal PutOnEvent(ControlPoint controlPoint, Vehicle vehicle)
            {
                ControlPoint = controlPoint;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                if (!ControlPoint.Accessible()) throw new ZeroVacancyException();
                Vehicle.Log(this);
                ControlPoint.At = Vehicle;
                ControlPoint.Locked = true;
                ControlPoint.HourCounter.ObserveCount(1, ClockTime);
            }
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
                if (Vehicle.Category.KeepTrack) Vehicle.Log(this);
                ControlPoint.At = Vehicle;
                ControlPoint.Locked = true;
                ControlPoint.HourCounter.ObserveCount(1, ClockTime);
            }
            public override string ToString() { return string.Format("{0}_Reach", ControlPoint); }
        }
        private class MoveEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal MoveEvent(ControlPoint controlPoint, Vehicle vehicle)
            {
                ControlPoint = controlPoint;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                if (ControlPoint.At != Vehicle) throw new VehicleIsNotAtException();
                if (Vehicle.Category.KeepTrack) Vehicle.Log(this);
                ControlPoint.At = null;
                ControlPoint.HourCounter.ObserveCount(0, ClockTime);
                Schedule(new ReleaseEvent(ControlPoint), TimeSpan.FromSeconds(
                    // time delay for releasing the control point
                    Math.Max(1, // min 1s
                    Vehicle.Category.SafetyLength / Math.Min(Vehicle.Speed, ControlPoint.IncomingSegments.Min(seg => seg.Path.Config.FullSpeed)) 
                    * 1.5 // safety factor for time
                    ))); 
            }
            public override string ToString() { return string.Format("{0}_MoveOut", ControlPoint); }
        }
        private class ReleaseEvent : Event
        {
            public ControlPoint ControlPoint { get; private set; }
            internal ReleaseEvent(ControlPoint controlPoint)
            {
                ControlPoint = controlPoint;
            }
            public override void Invoke()
            {
                ControlPoint.Locked = false;
                /*************** PULL WHEN VEHICLE RESTART TO MOVE ****************/
                // control point vacancy is released
                var paths = ControlPoint.Config.Positions.Keys;
                foreach (var seg in paths.Concat(paths.SelectMany(p=>p.Conflicts)).Distinct().SelectMany(p=>p.ControlPoints).Distinct()
                    // control points at connected paths and their conflicts
                    .SelectMany(cp=>ControlPoint.PathMover.ControlPoints[cp].IncomingSegments)
                    .Where(seg => seg.ReadyTime != null) // segment ready for vehicle to depart
                    .OrderBy(seg => seg.ReadyTime.Value)) // order by finish time
                    Execute(seg.Depart());
            }
            public override string ToString() { return string.Format("{0}_Release", ControlPoint); }
        }
        #endregion

        #region Input Events - Getters
        public Event Reach(Vehicle vehicle) { return new ReachEvent(this, vehicle); }
        public Event Move(Vehicle vehicle) { return new MoveEvent(this, vehicle); }
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
        public class ZeroVacancyException : Exception
        {
            public ZeroVacancyException() : base("Control Point is not accessible due to vacancy issue.") { }
        }
        #endregion

        public ControlPoint(Statics config, int seed = 0, string tag = null) : base(config, seed, tag)
        {
            Name = "ControlPoint";
            At = null;
            IncomingSegments = new List<Path.Segment>();
            OutgoingSegments = new List<Path.Segment>();
            HourCounter = new HourCounter();
        }

        public override void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }

        public override void WriteToConsole(DateTime? clockTime = null)
        {
            Console.Write("|{0}", this);
            if (At != null) Console.Write(":{0}", At);
            Console.Write("|");
        }
    }
}
