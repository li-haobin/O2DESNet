using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using O2DESNet.Drawing;
using System.Xml.Serialization;
using O2DESNet.Distributions;
using System.Diagnostics;

namespace O2DESNet.Traffic
{
    public class PathMover : State<PathMover.Statics>, IDrawable
    {
        #region Statics
        public class Statics : Scenario, IDrawable
        {
            public string Tag { get; set; }
            public Dictionary<string, Path.Statics> Paths { get; protected set; } = new Dictionary<string, Path.Statics>();
            public Dictionary<string, ControlPoint> ControlPoints { get; protected set; } = new Dictionary<string, ControlPoint>();
            public string RoutingTablesFile { get; set; } = null;
            public int Capacity
            {
                get
                {
                    int capacity = 0;
                    foreach (var path in Paths.Values)
                    {
                        if (path.Capacity > int.MaxValue - capacity) return int.MaxValue;
                        else capacity += path.Capacity;
                    }
                    return capacity;
                }
            }

            #region Path Mover Builder
            public ControlPoint CreateControlPoint(string tag = null)
            {
                CheckInitialized();
                var cp = new ControlPoint { Tag = tag, Index = ControlPoints.Count == 0 ? 0 : ControlPoints.Max(i => i.Value.Index) + 1 };
                if (cp.Tag == null || cp.Tag.Length == 0) cp.Tag = string.Format(" {0}", cp.Index);
                ControlPoints.Add(cp.Tag, cp);
                return cp;
            }
            public ControlPoint CreateControlPoint(double x, double y, string tag = null)
            {
                var cp = CreateControlPoint(tag);
                cp.X = x;
                cp.Y = y;
                return cp;
            }

            public Path.Statics CreatePath(string tag = null, double length = 0, int capacity = int.MaxValue, ControlPoint start = null, ControlPoint end = null, bool crossHatched = false)
            {
                CheckInitialized();
                var path = new Path.Statics
                {
                    //PathMover = this,
                    Tag = tag,
                    Index = Paths.Count == 0 ? 0 : Paths.Max(i => i.Value.Index) + 1,
                    Length = length,
                    Capacity = capacity,
                    CrossHatched = crossHatched,
                };
                if (path.Tag == null || path.Length == 0) path.Tag = string.Format("{0}", path.Index);
                if (start != null) { path.Start = start; start.PathsOut.Add(path); }
                if (end != null) { path.End = end; end.PathsIn.Add(path); }
                path.Trajectory.AddRange(new Point[] { new Point(start.X, start.Y), new Point(end.X, end.Y) });
                Paths.Add(path.Tag, path);
                return path;
            }
            public void Remove(Path.Statics path)
            {
                path.Start.PathsOut.Remove(path);
                path.End.PathsIn.Remove(path);
                Paths.Remove(path.Tag);
            }
            public void Remove(ControlPoint cp)
            {
                foreach (var p in cp.PathsOut.Concat(cp.PathsIn).ToList()) Remove(p);
                ControlPoints.Remove(cp.Tag);
            }
            private void CheckInitialized()
            {
                if (_initialized) throw new Exception("PathMover cannot be modified after initialization.");
            }
            #endregion

            #region For Static Routing (Distance-Based)
            private bool _initialized = false;
            public void Initialize()
            {
                if (!_initialized)
                {
                    ConstructRoutingTables();
                    _initialized = true;
                }
            }
            private void OutputRoutingTables()
            {
                var controlPoints = ControlPoints.Values.OrderBy(cp => cp.Index).ToList();
                if (RoutingTablesFile == null) return;
                if (!_initialized) Initialize();
                var str = string.Join(";", controlPoints.Select(cp => string.Format("{0}.{1}", cp.Index, string.Join(",", cp.RoutingTable.Where(i => i.Value != null).Select(i => string.Format("{0}:{1}", i.Key.Index, i.Value.Index))))));
                using (StreamWriter sw = new StreamWriter(RoutingTablesFile)) sw.Write(str);
            }
            private void CheckFeasibility()
            {
                foreach (var path in Paths.Values.Where(p => p.CrossHatched))
                {
                    foreach (var p in path.Start.PathsIn.Concat(path.End.PathsOut))
                        if (p.CrossHatched)
                            throw new Exception(string.Format("Consecutive CrossHatched Paths, i.e., {0} & {1}, is not allowed.", path, p));
                }
            }
            protected virtual void ConstructRoutingTables()
            {
                var to_at_next = ControlPoints.Values.ToDictionary(to => to, 
                    to => (Dictionary<ControlPoint, Tuple<ControlPoint, double>>)null);
                Parallel.ForEach(ControlPoints.Values, to =>
                {
                    to_at_next[to] = ControlPoints.Values.ToDictionary(cp => cp, 
                        cp => new Tuple<ControlPoint, double>(null, double.PositiveInfinity));
                    to_at_next[to][to] = new Tuple<ControlPoint, double>(null, 0);
                    var toProcess = new Queue<ControlPoint>(new ControlPoint[] { to });
                    while (toProcess.Count > 0)
                    {
                        var at = toProcess.Dequeue();
                        foreach (var path in at.PathsIn)
                        {
                            var start = path.Start;
                            var dist = to_at_next[to][at].Item2 + path.Length;
                            if (dist < to_at_next[to][start].Item2)
                            {
                                to_at_next[to][start] = new Tuple<ControlPoint, double>(at, dist);
                                toProcess.Enqueue(start);
                            }
                        }
                    }
                });
                foreach (var at in ControlPoints.Values)
                    at.RoutingTable = ControlPoints.Values.ToDictionary(to => to, to => to_at_next[to][at].Item1);
            }
            #endregion

            #region For Drawing
            public TransformGroup TransformGroup { get; } = new TransformGroup();

            private bool _showTag = true;
            protected Canvas _drawing = null;
            public Canvas Drawing { get { if (_drawing == null) UpdDrawing(); return _drawing; } }
            public bool ShowTag { get { return _showTag; } set { if (_showTag != value) { _showTag = value; UpdDrawing(); } } }
            public virtual void UpdDrawing(DateTime? clockTime = null)
            {
                if (_drawing == null) _drawing = new Canvas();
                foreach (var cp in ControlPoints.Values)
                {
                    cp.ShowTag = ShowTag;
                    if (cp.Drawing.Parent != null) ((Canvas)cp.Drawing.Parent).Children.Remove(cp.Drawing);
                    _drawing.Children.Add(cp.Drawing);
                    cp.UpdDrawing(clockTime);
                }
                foreach (var path in Paths.Values)
                {
                    path.ShowTag = ShowTag;
                    if (path.Drawing.Parent != null) ((Canvas)path.Drawing.Parent).Children.Remove(path.Drawing);
                    _drawing.Children.Add(path.Drawing);                    
                    path.UpdDrawing(clockTime);
                }
                _drawing.RenderTransform = TransformGroup;
            }
            #endregion

            #region XML
            public virtual void ToXML(string file = null) { XML.ToXML(this, file ?? string.Format("{0}.xml", Tag)); }
            public static Statics FromXML(string file) { return XMLParser<XML>.ReadFromXML(file).Restore(); }
            [XmlType("PathMover")]
            public class XML
            {
                public string Tag { get; set; }
                public List<ControlPoint.XML> ControlPoints { get; set; }
                public List<Path.Statics.XML> Paths { get; set; }
                public XML() { }
                public XML(Statics pm)
                {
                    Tag = pm.Tag;
                    ControlPoints = pm.ControlPoints.Values.OrderBy(cp => cp.Index).Select(cp => new ControlPoint.XML(cp)).ToList();
                    Paths = pm.Paths.Values.OrderBy(p => p.Start.Index).ThenBy(p => p.End.Index).Select(path => new Path.Statics.XML(path)).ToList();
                }
                public static void ToXML(Statics pm, string file)
                {
                    var xml = new XML(pm);
                    XMLParser<XML>.WriteToXML(xml, file);
                }
                protected Dictionary<int, ControlPoint> _idxCps;
                public virtual Statics Restore()
                {
                    var cfg = new Statics { Tag = Tag };
                    _idxCps = ControlPoints.Select(xml => xml.Restore()).ToDictionary(cp => cp.Index, cp => cp);
                    cfg.ControlPoints = _idxCps.Values.ToDictionary(cp => cp.Tag, cp => cp);
                    cfg.Paths = Paths.Select(xml => xml.Restore(_idxCps)).ToDictionary(path => path.Tag, path => path);

                    Func<string, ControlPoint> idxCp = idx => _idxCps[Convert.ToInt32(idx, 16)];
                    foreach (var xml in ControlPoints)
                    {
                        var cp = idxCp(xml.Index);
                        var allCps = new HashSet<ControlPoint>(_idxCps.Values);
                        var nextCps = new HashSet<ControlPoint>(cp.PathsOut.Select(p => p.End));
                        allCps.Remove(cp);
                        ControlPoint next;
                        foreach (var line in xml.Router.Select(s => s.Split('@')))
                        {
                            next = idxCp(line[0]);
                            cp.RoutingTable.Add(next, next);
                            nextCps.Remove(next);
                            allCps.Remove(next);
                            foreach (var target in line[1].Split('|').Where(s => s.Length > 0).Select(s => idxCp(s)))
                            {
                                cp.RoutingTable.Add(target, next);
                                allCps.Remove(target);
                            }
                        }
                        // for the complement info
                        if (xml.Router.Count > 0 && nextCps.Count > 1) throw new Exception("Wrong size for routing information.");
                        if (cp.PathsOut.Count > 0)
                        {
                            next = nextCps.First();
                            cp.RoutingTable.Add(next, next);
                            allCps.Remove(next);
                            foreach (var target in allCps) cp.RoutingTable.Add(target, next);
                        }
                        else
                        {
                            //throw new Exception(string.Format("No path out at Control Point {0}.", cp.Tag)); // ControlPoints might not be connected
                        }
                    }

                    return cfg;
                }
            }
            #endregion
        }
        #endregion

        #region Dynamics
        public Dictionary<Path.Statics, Path> Paths { get; private set; } = new Dictionary<Path.Statics, Path>();
        public HashSet<IVehicle> Vehicles { get; private set; } = new HashSet<IVehicle>();
        public Dictionary<IVehicle, DateTime> Timestamps { get; private set; } = new Dictionary<IVehicle, DateTime>();
        public Dictionary<Path, Queue<IVehicle>> DepartingQueues { get; private set; } = new Dictionary<Path, Queue<IVehicle>>();
        public HourCounter HCounter_Departing { get; private set; } = new HourCounter();
        public HourCounter HCounter_Travelling { get; private set; } = new HourCounter();
        public double TotalMilage { get; private set; }
        public TimeSpan TotalVehicleTime { get; private set; } = new TimeSpan();
        public double Utilization { get { return HCounter_Travelling.AverageCount / Config.Capacity; } }
        /// <summary>
        /// km/h
        /// </summary>
        public double AverageSpeed { get { return TotalMilage / TotalVehicleTime.TotalHours; } }
        public Dictionary<ControlPoint, bool> ToArrive { get; private set; }
        public HourCounter HCounter_Deadlocks { get; private set; } = new HourCounter();
        public List<Path> DeadlockedPaths { get; private set; } = new List<Path>();
        #endregion

        #region Events
        private abstract class InternalEvent : Event<PathMover, Statics> { }

        // Alpha_1
        private class CallToDepartEvent : InternalEvent
        {
            internal IVehicle Vehicle { get; set; }
            internal ControlPoint At { get; set; }
            public override void Invoke()
            {
                while (At.Equals(Vehicle.Targets.FirstOrDefault())) Execute(Vehicle.RemoveTarget());
                if (Vehicle.Targets.Count == 0)
                {
                    try
                    {
                        Execute(This.OnDepart.Select(e => e(Vehicle, At)));
                        Execute(This.OnArrive.Select(e => e(Vehicle, At)));
                        return;
                    }
                    catch(Exception e)
                    {
                        throw e;
                        //throw new Exception("Vechile must have at least one target that is different from the departing point.");
                    }
                }
                var path = This.Paths[At.PathTo(Vehicle.Targets.First())];
                This.DepartingQueues[path].Enqueue(Vehicle);
                This.HCounter_Departing.ObserveChange(1, ClockTime);
                This.Timestamps.Add(Vehicle, ClockTime);
                Execute(new DepartEvent { Path = path });
            }
        }

        // Alpha_2
        private class UpdToArriveEvent : InternalEvent
        {
            internal ControlPoint At { get; set; }
            internal bool ToArrive { get; set; }
            public override void Invoke()
            {
                This.ToArrive[At] = ToArrive;
                foreach (var path in At.PathsIn.Select(p => This.Paths[p]))
                {
                    Execute(path.UpdToExit(path, This.ToArrive[At]));
                    if (path.Config.CrossHatched)
                    {
                        foreach (var prev in path.Config.Start.PathsIn.Select(p => This.Paths[p]))
                            Execute(prev.UpdToExit(path, This.ToArrive[At]));
                    }
                }
            }
        }

        // Alpha_3
        private class ResetEvent : InternalEvent
        {
            public override void Invoke()
            {
                foreach (var path in This.Paths.Values)
                {
                    Execute(path.Reset());
                    This.DepartingQueues[path] = new Queue<IVehicle>();
                }
                This.Vehicles = new HashSet<IVehicle>();
                This.HCounter_Departing.ObserveCount(0, ClockTime);
                This.HCounter_Travelling.ObserveCount(0, ClockTime);
            }
        }

        // Beta_1
        private class DepartEvent : InternalEvent
        {
            internal Path Path { get; set; }
            public override void Invoke()
            {                
                if (This.DepartingQueues[Path].Count == 0 || Path.Vacancy == 0) return;
                var vehicle = This.DepartingQueues[Path].Dequeue();
                This.Vehicles.Add(vehicle);
                Execute(new ReachControlPointEvent { Vehicle = vehicle, At = Path.Config.Start });
                This.HCounter_Departing.ObserveChange(-1, ClockTime);
                This.HCounter_Travelling.ObserveChange(1, ClockTime);
                Execute(This.OnDepart.Select(e => e(vehicle, Path.Config.Start)));
                Execute(new DepartEvent { Path = Path });
            }
        }

        // Beta_2
        private class ReachControlPointEvent : InternalEvent
        {
            internal IVehicle Vehicle { get; set; }
            internal ControlPoint At { get; set; }
            public override void Invoke()
            {
                if (At.Equals(Vehicle.Targets.First()))
                {
                    Execute(Vehicle.RemoveTarget());
                    if (Vehicle.Targets.Count == 0)
                    {
                        Execute(new ArriveEvent { Vehicle = Vehicle, At = At });
                        return;
                    }
                }
                var path = This.Paths[At.PathTo(Vehicle.Targets.First())];
                Execute(path.Enter(Vehicle));
                if (!This.PathsToDraw.Contains(path)) This.PathsToDraw.Add(path); //for drawing
            }
        }

        // Beta_3
        private class ArriveEvent : InternalEvent
        {
            internal IVehicle Vehicle { get; set; }
            internal ControlPoint At { get; set; }
            public override void Invoke()
            {
                This.Vehicles.Remove(Vehicle);
                This.HCounter_Travelling.ObserveChange(-1, ClockTime);
                This.Timestamps.Remove(Vehicle);
                Execute(This.OnArrive.Select(e => e(Vehicle, At)));
            }
        }

        // Beta_4
        private class ChkDeadlockEvent : InternalEvent
        {
            internal Path Path { get; set; }
            public override void Invoke()
            {
                if (LockedBy(Path) != null)
                {
                    var paths = new List<Path> { LockedBy(Path) };
                    var hashset = new HashSet<Path>();

                    while (!paths.Last().Equals(Path))
                    {
                        var path = LockedBy(paths.Last());
                        if (path == null)
                        {
                            This.DeadlockedPaths = new List<Path>();
                            return;
                        }
                        if (!hashset.Contains(path))
                        {
                            hashset.Add(path);
                            paths.Add(path);
                        }
                        else
                        {
                            while (paths.First() != path) paths.RemoveAt(0);
                            break;
                        }
                    }
                    This.DeadlockedPaths = paths;
                    Execute(This.OnDeadlock.Select(e => e()));
                    This.HCounter_Deadlocks.ObserveChange(1, ClockTime);
                }
            }
            private Path LockedBy(Path path)
            {
                if (!path.Locked) return null;
                var target = path.VehiclesCompleted.First().Targets.First();
                if (target == path.Config.End)
                {
                    if (path.Occupancy == path.Config.Capacity && !This.ToArrive[path.Config.End] /// cannot exit
                        && path.Config.End.PathsOut.Count(p => This.Paths[p].Config.Capacity == This.Paths[p].Occupancy) > 0) /// the subsequent paths cannot be accessed
                        return This.Paths[path.Config.End.PathsOut.First(p => This.Paths[p].Config.Capacity == This.Paths[p].Occupancy)];
                    return null;
                }
                var next = This.Paths[path.Config.End.PathTo(target)];
                if (target == next.Config.End && next.Config.CrossHatched)
                {
                    if (!This.ToArrive[next.Config.End] /// cannot exit
                        && next.Config.End.PathsOut.Count(p => This.Paths[p].Config.Capacity == This.Paths[p].Occupancy) > 0) /// the subsequent paths cannot be accessed
                        return This.Paths[next.Config.End.PathsOut.First(p => This.Paths[p].Config.Capacity == This.Paths[p].Occupancy)];
                    return null;
                }
                if (next.Config.CrossHatched) next = This.Paths[next.Config.End.PathTo(target)];
                return next;
            }
        }
        private class CalcMilageEvent : InternalEvent
        {
            internal Path Path { get; set; }
            internal IVehicle Vehicle { get; set; }
            public override void Invoke()
            {
                This.TotalMilage += Path.Config.Length;
                This.TotalVehicleTime += ClockTime - This.Timestamps[Vehicle];
                This.Timestamps[Vehicle] = ClockTime;
                Execute(Vehicle.CalcMilage(Path.Config.Length));
            }
        }
        private class TeleportEvent : InternalEvent
        {
            public override void Invoke()
            {
                if (This.DeadlockedPaths.Count == 0) return;

                var from = Uniform.Sample(DefaultRS, This.DeadlockedPaths
                    .Where(p => p.VehiclesCompleted.Count > 0)); /// in case the deadlock is joined by an inaccessible work point
                var vehicle = from.VehiclesCompleted.First();
                Execute(from.Dequeue());

                var to = Uniform.Sample(DefaultRS, This.Paths.Values.Except(This.DeadlockedPaths).Where(p => p.Vacancy > 0 && !p.Config.CrossHatched));
                Execute(to.Enter(vehicle));
            }
        }
        #endregion

        #region Input Events - Getters
        public virtual Event CallToDepart(IVehicle vehicle, ControlPoint at) { return new CallToDepartEvent { This = this, Vehicle = vehicle, At = at }; }
        public virtual Event UpdToArrive(ControlPoint at, bool toArrive) { return new UpdToArriveEvent { This = this, At = at, ToArrive = toArrive }; }
        /// <summary>
        /// Reset the PathMover by removing all vehicles, and releasing locks caused by congested paths.
        /// </summary>
        public virtual Event Reset() { return new ResetEvent { This = this }; }
        public virtual Event Teleport() { return new TeleportEvent { This = this }; }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<IVehicle, ControlPoint, Event>> OnDepart { get; private set; } = new List<Func<IVehicle, ControlPoint, Event>>();
        public List<Func<IVehicle, ControlPoint, Event>> OnArrive { get; private set; } = new List<Func<IVehicle, ControlPoint, Event>>();
        public List<Func<Event>> OnDeadlock { get; private set; } = new List<Func<Event>>();
        #endregion

        public PathMover(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "PathMover";
            foreach (var pathConfig in Config.Paths.Values)
            {
                var path = new Path(pathConfig, DefaultRS.Next(), tag: pathConfig.Tag);
                Paths.Add(pathConfig, path);
                DepartingQueues.Add(path, new Queue<IVehicle>());
                path.OnExit.Add(veh => new CalcMilageEvent { This = this, Path = path, Vehicle = veh });
                path.OnExit.Add(veh => new ReachControlPointEvent { This = this, Vehicle = veh, At = path.Config.End });
                path.OnVacancyChg.Add(() => new DepartEvent { This = this, Path = path });
                //path.OnExit.Add(veh => new DepartEvent { This = this, Path = path });
            }
            foreach (var path in Paths.Values)
            {
                foreach (var prev in path.Config.Start.PathsIn.Select(p => Paths[p]))
                {
                    path.OnVacancyChg.Add(() => prev.UpdToEnter(path));
                    InitEvents.Add(prev.UpdToEnter(path));
                    InitEvents.Add(path.UpdToExit(path, true));
                    // cross-hatching
                    if (prev.Config.CrossHatched)
                        foreach (var prev2 in prev.Config.Start.PathsIn.Select(p => Paths[p]))
                        {
                            path.OnVacancyChg.Add(() => prev2.UpdToEnter(path));
                            InitEvents.Add(prev2.UpdToEnter(path));
                        }
                    if (path.Config.CrossHatched)
                    {
                        InitEvents.Add(prev.UpdToExit(path, true));
                    }
                    path.OnLockedByPaths.Add(() => new ChkDeadlockEvent { This = this, Path = path });
                }
            }
            ToArrive = Config.ControlPoints.Values.ToDictionary(cp => cp, cp => true);
        }

        public override void WarmedUp(DateTime clockTime)
        {
            foreach (var path in Paths.Values) path.WarmedUp(clockTime);
            HCounter_Departing.WarmedUp(clockTime);
            HCounter_Travelling.WarmedUp(clockTime);
            HCounter_Deadlocks.WarmedUp(clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime = default(DateTime?))
        {
            foreach (var path in Paths.Values) path.WriteToConsole();
        }

        #region For Drawing
        private DateTime? _timestamp = null;
        protected Canvas _drawing = null;

        public TransformGroup TransformGroup { get { return Config.TransformGroup; } }
        public Canvas Drawing { get { if (_drawing == null) { InitDrawing(); UpdDrawing(); } return _drawing; } }
        public bool ShowTag
        {
            get { return Config.ShowTag; }
            set { if (Config.ShowTag != value) { Config.ShowTag = value; UpdDrawing(_timestamp); } }
        }
        protected virtual void InitDrawing()
        {
            _drawing = Config.Drawing;
            //_drawing = new Canvas();
            //foreach (var cp in Config.ControlPoints.Values) _drawing.Children.Add(cp.Drawing);
            foreach (var path in Paths.Values) _drawing.Children.Add(path.Drawing);
            //_drawing.RenderTransform = Config.TransformGroup;
        }
        private HashSet<Path> PathsToDraw { get; } = new HashSet<Path>();
        public void UpdDrawing(DateTime? clockTime = null)
        {
            List<Path> toRemove = new List<Path>();            
            foreach (var path in PathsToDraw)
            {
                path.ShowTag = ShowTag;
                path.UpdDrawing(clockTime);
                if (path.HC_AllVehicles.LastCount == 0) lock (toRemove) toRemove.Add(path);
            }
            //PathsToDraw.RemoveWhere(p => p.HC_AllVehicles.LastCount == 0);
            foreach (var path in toRemove) PathsToDraw.Remove(path);
        }
        #endregion

    }
}
