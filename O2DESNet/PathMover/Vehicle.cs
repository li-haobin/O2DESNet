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
        internal Path.Segment Segment { get; set; } = null;
        #endregion

        #region Statics
        public class Statics : Scenario
        {
            public string Name { get; set; }
            public double Speed { get; set; }

            /// <summary>
            /// Timestamps are recorded for tracking the movement of the vehicle
            /// </summary>
            public bool KeepTrack { get; set; }

            // for graphical display
            public double Length { get; set; } = 3.95;
            public double Width { get; set; } = 1.67;

            public string Color { get; set; } = "white";

            public Statics() { Name = Guid.NewGuid().ToString().Substring(0, 6); }
            public SVG Graph(double scale)
            {
                var svg = new SVG();

                svg.Id = string.Format("vehCate_{0}", Name);
                svg.Styles.AddRange(SVGStyles);
                svg.Reference = new Point(Length * scale / 2, Width * scale / 2);

                svg.Body = string.Format("<g id=\"{0}\">\n", svg.Id);
                svg.Body += string.Format("<rect width=\"{0}\" height=\"{1}\" stroke=\"black\" fill=\"" + Color + "\" fill-opacity=\"0.5\"/>\n", Length * scale, Width * scale);
                svg.Body += SVG.GetText(classId: "vehCate_label", text: Name, reference: new Point(0, -4), posture: new Tuple<Point, double>(new Point(Length * scale / 2, Width * scale / 2), 0));
                svg.Body += "</g>\n";

                return svg;
            }
            public static List<SVG.Style> SVGStyles
            {
                get
                {
                    return new List<SVG.Style> {
                        new SVG.Style("vehCate_label",
                            new SVG.Attr("text-anchor", "middle"),
                            new SVG.Attr("font-family", "Verdana"),
                            new SVG.Attr("font-size", "8px"),
                            new SVG.Attr("fill", "darkgreen")
                            ),
                        new SVG.Style("vehCate_sign",
                            new SVG.Attr("text-anchor", "middle"),
                            new SVG.Attr("font-family", "Verdana"),
                            new SVG.Attr("font-size", "9px"),
                            new SVG.Attr("fill", "red")
                            ),
                    };
                }
            }
        }
        #endregion

        #region Dynamics
        public virtual double Speed { get { return Category.Speed; } }
        public List<ControlPoint> Targets { get; private set; } = new List<ControlPoint>();
        public ControlPoint Current { get; private set; } = null;        
        public ControlPoint Next
        {
            get
            {
                if (Targets.Count > 0)
                    return Current.PathMover.ControlPoints[Current.Config.RoutingTable[Targets.First().Config]];
                else return null;
            }
        }
        public Path PathToNext
        {
            get
            {
                if (Targets.Count > 0 && Targets.First() != Current)
                    return Current.PathMover.Paths[Current.Config.GetPathFor(Targets.First().Config)];
                else return null;
            }
        }

        public List<Tuple<DateTime, Tuple<Point, double>>> Postures { get; private set; } = new List<Tuple<DateTime, Tuple<Point, double>>>();
        public Tuple<Point, double> GetPosture(DateTime clockTime)
        {
            if (Current == null) return null;
            var segment = Segment != null ? Segment : Current.IncomingSegments
                .Where(seg => seg.Path.Config.Coords != null && seg.Path.Config.Coords.Count > 0).FirstOrDefault();
            if (segment == null) return null;
             
            var served = segment.Served;
            var serving = segment.Serving;

            if (!served.Contains(this) && !serving.Contains(this))  // when the vehicle is on the control point
                return Point.SlipOnCurve(segment.Path.Config.Coords, segment.EndRatio);
            else
            {
                var positions = segment.Sequence.Select(veh => segment.StartRatio + (segment.Forward ? 1 : -1) *
                    (clockTime - segment.StartTimes[veh]).TotalSeconds * // time lapsed since enter
                    Math.Min(veh.Category.Speed, segment.Path.Config.FullSpeed) // speed taken
                    / segment.Path.Config.Length
                    ).ToList();
                var startTime = segment.StartTimes[this];

                var vehLength = Category.Length / segment.Path.Config.Length; // ratio gap occupied by vehicle length
                var ratio = segment.EndRatio;
                if (Next.At != null) ratio -= (segment.Forward ? 1 : -1) * vehLength;
                for(int i = 0; i < segment.Sequence.Count; i++)
                {
                    ratio = segment.Forward ? Math.Min(ratio, positions[i]) : Math.Max(ratio, positions[i]);
                    if (segment.Sequence[i] == this)
                    {

                        return Point.SlipOnCurve(segment.Path.Config.Coords, ratio);
                    }
                    ratio -= (segment.Forward ? 1 : -1) * vehLength;
                }
                throw new Exception("Vehicle not exist in sequence of FIFO Server.");
            }
        }
        public enum State { Travelling, Parking }
        public List<Tuple<DateTime, State>> StateHistory { get; private set; } = new List<Tuple<DateTime, State>>();
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
                Vehicle.Current = ControlPoint;
                ControlPoint.PathMover.Vehicles.Add(Vehicle);
                Vehicle.StateHistory.Add(new Tuple<DateTime, State>(ClockTime, State.Parking));

                // add vehicle posture
                Vehicle.Postures.Add(new Tuple<DateTime, Tuple<Point, double>>(ClockTime, Vehicle.GetPosture(ClockTime)));

                Execute(new ControlPoint.PutOnEvent(ControlPoint, Vehicle));
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
                if (Vehicle.Targets.Count > 0) throw new VehicleStatusException("'Targets' must be empty on PutOff event.");
                if (Vehicle.Current == null) throw new VehicleStatusException("'Current' cannot be null on PutOff event.");
                if (Vehicle.Category.KeepTrack) Vehicle.Log(this);
                Vehicle.Current.PathMover.Vehicles.Remove(Vehicle);
                Vehicle.Current = null;
            }
            public override string ToString() { return string.Format("{0}_PutOff", Vehicle); }
        }
        private class MoveEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal MoveEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                Vehicle.Log(this);
                Execute(Vehicle.Current.Move(Vehicle));
            }
            public override string ToString() { return string.Format("{0}_Move", Vehicle); }
        }
        private class ReachEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal ReachEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                var next = Vehicle.Next;
                Vehicle.Log(this);
                Execute(next.Reach(Vehicle));
                Vehicle.Current = next;

                while (Vehicle.Current == Vehicle.Targets.FirstOrDefault()) Vehicle.Targets.RemoveAt(0);                
            }
            public override string ToString() { return string.Format("{0}_Reach", Vehicle); }
        }
        private class MoveToEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public List<ControlPoint> Targets { get; private set; }
            internal MoveToEvent(Vehicle vehicle, List<ControlPoint> targets)
            {
                Vehicle = vehicle;
                Targets = targets;
            }
            public override void Invoke()
            {
                if (Vehicle.Current == null) throw new VehicleStatusException("'Current' cannot be null on MoveTo event.");
                Vehicle.Log(this);
                if (Vehicle.Targets.Count > 0) Vehicle.Targets.AddRange(Targets);
                else
                {
                    Vehicle.Targets.AddRange(Targets);
                    while (Vehicle.Current == Vehicle.Targets.FirstOrDefault()) Vehicle.Targets.RemoveAt(0);
                    if (Vehicle.Targets.Count > 0)
                    {
                        Execute(Vehicle.PathToNext.Move(Vehicle));
                        Vehicle.StateHistory.Add(new Tuple<DateTime, State>(ClockTime, State.Travelling));
                    }
                    else Execute(Vehicle.Complete());
                }
            }
            public override string ToString() { return string.Format("{0}_MoveTo", Vehicle); }
        }
        private class ClearTargetsEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal ClearTargetsEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {
                if (Vehicle.Targets.Count > 0) Vehicle.Targets = new List<ControlPoint> { Vehicle.Targets.First() };
            }
            public override string ToString() { return string.Format("{0}_ClearTargets", Vehicle); }
        }
        private class CompleteEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            internal CompleteEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke()
            {                
                foreach (var evnt in Vehicle.OnComplete) Execute(evnt());
                Vehicle.StateHistory.Add(new Tuple<DateTime, State>(ClockTime, State.Parking));
            }
            public override string ToString() { return string.Format("{0}_Complete", Vehicle); }
        }
        #endregion

        #region Input Events - Getters
        public Event PutOn(ControlPoint current) { return new PutOnEvent(this, current); }
        public Event PutOff() { return new PutOffEvent(this); }
        /// <summary>
        /// Move the vehicle to a list of targets in sequence, append to the list if there are existing targets
        /// </summary>
        public Event MoveTo(List<ControlPoint> targets) { return new MoveToEvent(this, targets); }
        /// <summary>
        /// Clear all targets except the first one which is being executed.
        /// </summary>
        public Event ClearTargets() { return new ClearTargetsEvent(this); }
        
        // Moving from control point to control point
        internal Event Move() { return new MoveEvent(this); }
        internal Event Reach() { return new ReachEvent(this); }
        internal Event Complete() { return new CompleteEvent(this); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Event>> OnComplete { get; private set; }
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
            OnComplete = new List<Func<Event>>();
        }

        public override void Log(Event evnt) { if (Category.KeepTrack) base.Log(evnt); }

        public override void WarmedUp(DateTime clockTime) { }

        public override void WriteToConsole(DateTime? clockTime)
        {
            Console.Write("{0}:\t", this);
            var posture = GetPosture(clockTime.Value);
            if (posture != null) Console.Write("[{0:F2},{1:F2},{2:F2}]\t", posture.Item1.X, posture.Item1.Y, posture.Item2);
            if (Targets.Count > 0 && Targets.First() != Current)
            {
                Console.Write("{0} - {1}", Current, PathToNext);
                //if (Segment.Delayed.Contains(this)) Console.Write("!");
                if (Segment.Served.Contains(this)) Console.Write("!!");
                Console.Write(" -> {0}\t", Next);
                Console.Write("*");
                foreach (var cp in Targets) Console.Write("{0} ", cp);
                Console.WriteLine();
            }
            else Console.WriteLine(Current);
        }
    }
}
