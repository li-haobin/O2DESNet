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
            public double SafetyLength { get { return Length * (1 + DistanceSafetyFactor); } }
            public double Width { get; set; } = 1.67;
            public double DistanceSafetyFactor { get; set; } = 0.15;

            public string Color { get; set; } = "green";

            public Statics() { Name = Guid.NewGuid().ToString().ToUpper().Substring(0, 4); }

            #region SVG Output
            public Group SVG()
            {
                string veh_cate_name = "veh_cate#" + Name;
                var g = new Group(veh_cate_name,
                    new Rect(-Length / 0.2, -Width / 0.2, Length * 10, Width * 10, "black", Color, new XAttribute("fill-opacity", 0.5)),
                    new Text(Label, Name, new XAttribute("transform", "translate(0 4.5)"))
                    );
                var label = new Text(Label, Name, new XAttribute("transform", "translate(0 4.5)"));
                return g;
            }

            public static CSS Label = new CSS("pm_vehCate_label", new XAttribute("text-anchor", "middle"), new XAttribute("font-family", "Verdana"), new XAttribute("font-size", "9px"), new XAttribute("fill", "black"));
            public static CSS RedLabel = new CSS("pm_vehCate_red_label", new XAttribute("text-anchor", "middle"), new XAttribute("font-family", "Verdana"), new XAttribute("font-size", "9px"), new XAttribute("fill", "red"));

            /// <summary>
            /// Including arrows, styles
            /// </summary>
            public static Definition SVGDefs { get { return new Definition(new Style(Label, RedLabel)); } }
            #endregion           
        }
        #endregion

        #region Dynamics
        public PathMover PathMover { get; private set; }
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
        
        public enum State { Travelling, Parking }
        public List<Tuple<double, State>> StateHistory { get; private set; } = new List<Tuple<double, State>> { new Tuple<double, State>(0, State.Parking) };
        public void LogState(DateTime clockTime, State state) { StateHistory.Add(new Tuple<double, State>((clockTime - PathMover.StartTime).TotalSeconds, state)); }
        public void ResetStateHistory() { StateHistory = new List<Tuple<double, State>> { new Tuple<double, State>(0, StateHistory.Last().Item2) }; }
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
                Vehicle.PathMover = ControlPoint.PathMover;
            }
            public override void Invoke()
            {
                if (Vehicle.Current != null) throw new VehicleStatusException("'Current' must be null on PutOn event.");                
                Vehicle.Current = ControlPoint;
                ControlPoint.PathMover.Vehicles.Add(Vehicle);
                Vehicle.StateHistory.Add(new Tuple<double, State>((ClockTime - ControlPoint.PathMover.StartTime).TotalSeconds, State.Parking));

                // add vehicle posture
                //Vehicle.Postures.Add(new Tuple<DateTime, Tuple<Point, double>>(ClockTime, Vehicle.GetPosture(ClockTime)));

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
                Vehicle.PathMover = null;
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
                        var current = Vehicle.Current;
                        Execute(Vehicle.PathToNext.Move(Vehicle));
                        Vehicle.LogState(ClockTime, State.Travelling);
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
                Vehicle.LogState(ClockTime, State.Parking);
            }
            public override string ToString() { return string.Format("{0}_Complete", Vehicle); }
        }
        private class UpdatePositionsInSegmentEvent : Event
        {
            public Vehicle Vehicle { get; private set; }
            public UpdatePositionsInSegmentEvent(Vehicle vehicle) { Vehicle = vehicle; }
            public override void Invoke() { Vehicle.Segment.LogAnchors(null, ClockTime); }
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
        internal Event UpdatePositionsInSegment() { return new UpdatePositionsInSegmentEvent(this); }
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

        #region SVG Output
        public Group SVG()
        {
            var g = new Group("veh#" + Id,
                new Use("veh_cate#" + Category.Name),
                new Text(Statics.Label, "VEH#" + Id, new XAttribute("transform", string.Format("translate(0 {0})", -Category.Width / 0.2 - 5)))
                );

            g.Add(new Group(
                new Text(Statics.RedLabel, "PARKING", 
                    new XAttribute("transform", string.Format("translate(0 {0})", Category.Width / 0.2 + 12))),
                    StateHistory.Count > 1 ? (XElement)
                    new Animate("visibility", StateHistory.Select(s => s.Item1), StateHistory.Select(s => s.Item2 == State.Parking ? "visible" : "hidden"), new XAttribute("fill", "freeze")) :
                    new Set("visibility", StateHistory.First().Item2 == State.Parking ? "visible" : "hidden", StateHistory.First().Item1)
                ));

            if (Anchors.Count > 0)
            {
                double begin = Anchors.First().Item1;
                Path.Statics path = Anchors.First().Item2;
                List<double> keyTimes = new List<double> { 0 }, keyPoints = new List<double> { Anchors.First().Item3 };
                List<double> offTimes = new List<double> { 0 }, onTimes = new List<double> { begin };
                int i = 0;
                while (true)
                {
                    if (++i == Anchors.Count || Anchors[i].Item2 != path)
                    {
                        g.Add(new AnimateMotion(string.Format("path#{0}_d", path.Index), new XAttribute("begin", string.Format("{0}s", begin)), new XAttribute("dur", string.Format("{0}s", keyTimes.Last())), new XAttribute("rotate", "auto"), new XAttribute("keyTimes", string.Join(";", keyTimes.Select(t => (t - keyTimes.First()) / keyTimes.Last()))), new XAttribute("keyPoints", string.Join(";", keyPoints)), new XAttribute("calcMode", "linear")));
                        if (i == Anchors.Count) break;
                        offTimes.Add(begin + keyTimes.Last());
                        onTimes.Add(Anchors[i].Item1);
                        begin = Anchors[i].Item1;
                        path = Anchors[i].Item2;
                        keyTimes = new List<double> { 0 };
                        keyPoints = new List<double> { Anchors[i].Item3 };
                    }
                    else
                    {
                        keyTimes.Add(Anchors[i].Item1 - begin);
                        keyPoints.Add(Anchors[i].Item3);
                    }
                }

                //put off when vehicle anchor breaks between two paths
                i = 1;
                while (i < onTimes.Count)
                {
                    if (onTimes[i] == offTimes[i])
                    {
                        onTimes.RemoveAt(i);
                        offTimes.RemoveAt(i);
                    }
                    else i++;
                }
                var values = new List<string>();
                keyTimes = new List<double>();
                for (i = 0; i < offTimes.Count; i++)
                {
                    values.Add("hidden");
                    values.Add("visible");
                    keyTimes.Add(offTimes[i]);
                    keyTimes.Add(onTimes[i]);
                }
                values.Add("hidden");
                keyTimes.Add(Anchors.Last().Item1);
                g.Add(new Animate("visibility", keyTimes, values, new XAttribute("fill", "freeze")));
            }
            return g;
        }

        private List<Tuple<double, Path.Statics, double>> Anchors { get; set; } = new List<Tuple<double, Path.Statics, double>>();
        public void LogAnchor(double time, Path.Statics path, double ratio)
        {
            if (Anchors.Count > 0 && time == Anchors.Last().Item1 && path == Anchors.Last().Item2) Anchors.RemoveAt(Anchors.Count - 1);            
            Anchors.Add(new Tuple<double, Path.Statics, double>(time, path, Math.Min(1, Math.Max(0, ratio))));
        }
        public void ResetAnchors() { Anchors = new List<Tuple<double, Path.Statics, double>>(); }
        #endregion
}
}
