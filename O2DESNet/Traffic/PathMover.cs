using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace O2DESNet.Traffic
{
    public class PathMover : State<PathMover.Statics>, IDrawable
    {
        #region Statics
        public class Statics : Scenario, IDrawable
        {
            public int Index { get; set; }
            public Dictionary<string, Path.Statics> Paths { get; private set; } = new Dictionary<string, Path.Statics>();
            public Dictionary<string, ControlPoint> ControlPoints { get; private set; } = new Dictionary<string, ControlPoint>();
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
            public ControlPoint CreatControlPoint(string tag = null)
            {
                CheckInitialized();
                var cp = new ControlPoint { Tag = tag, Index = ControlPoints.Count };
                if (cp.Tag == null || cp.Tag.Length == 0) cp.Tag = string.Format(" {0}", cp.Index);
                ControlPoints.Add(cp.Tag, cp);
                return cp;
            }

            public Path.Statics CreatePath(string tag = null, double length = 0, int capacity = int.MaxValue, ControlPoint start = null, ControlPoint end = null, bool crossHatched = false)
            {
                CheckInitialized();
                var path = new Path.Statics
                {
                    PathMover = this,
                    Tag = tag,
                    Index = Paths.Count,
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

            private void CheckInitialized()
            {
                if (_initialized) throw new Exception("PathMover cannot be modified after initialization.");
            }
            #endregion

            #region For Static Routing (Distance-Based)
            private bool _initialized = false;
            internal void Initialize()
            {
                if (!_initialized)
                {
                    ConstructRoutingTables();
                    _initialized = true;
                }
            }
            public void OutputRoutingTables()
            {
                var controlPoints = ControlPoints.Values.OrderBy(cp => cp.Index).ToList();
                if (RoutingTablesFile == null) return;
                if (!_initialized) Initialize();
                var str = string.Join(";", controlPoints.Select(cp => string.Format("{0}.{1}", cp.Index, string.Join(",", cp.RoutingTable.Where(i => i.Value != null).Select(i => string.Format("{0}:{1}", i.Key.Index, i.Value.Index))))));
                using (StreamWriter sw = new StreamWriter(RoutingTablesFile)) sw.Write(str);
            }
            private void ConstructRoutingTables()
            {
                var controlPoints = ControlPoints.Values.OrderBy(cp => cp.Index).ToList();
                var paths = Paths.Values.ToList();
                if (RoutingTablesFile != null)
                {
                    try
                    {
                        var str = File.ReadAllText(RoutingTablesFile);
                        foreach (var rt in str.Split(';'))
                        {
                            var ln = rt.Split('.');
                            int index = Convert.ToInt32(ln[0]);
                            if (ln[1].Length > 0) controlPoints[index].RoutingTable = ln[1].Split(',').Select(r => r.Split(':')).ToDictionary(r => controlPoints[Convert.ToInt32(r[0])], r => controlPoints[Convert.ToInt32(r[1])]);
                            else controlPoints[index].RoutingTable = new Dictionary<ControlPoint, ControlPoint>();
                            foreach (var cp in controlPoints)
                                if (cp != controlPoints[index] && !controlPoints[index].RoutingTable.ContainsKey(cp))
                                    controlPoints[index].RoutingTable.Add(cp, null);
                        }
                        return;
                    }
                    catch { }
                }
                foreach (var cp in controlPoints) cp.RoutingTable = new Dictionary<ControlPoint, ControlPoint>();
                var incompleteSet = controlPoints.ToList();
                var edges = paths.Select(path => new Tuple<int, int, double>(path.Start.Index, path.End.Index, path.Length)).ToList();
                while (incompleteSet.Count > 0)
                {
                    Parallel.ForEach(incompleteSet, cp => ConstructRoutingTables(cp.Index, edges));
                    //ConstructRoutingTables(incompleteSet.First().Index, edges);
                    incompleteSet.RemoveAll(cp => cp.RoutingTable.Count == ControlPoints.Count - 1);
                }
            }
            private void ConstructRoutingTables(int sourceIndex, List<Tuple<int, int, double>> edges)
            {
                var controlPoints = ControlPoints.Values.OrderBy(cp => cp.Index).ToList();
                var edgeList = edges.ToList();
                var dijkstra = new Dijkstra(edges);

                var sinkIndices = new HashSet<int>(controlPoints.Select(cp => cp.Index));
                sinkIndices.Remove(sourceIndex);
                foreach (var target in controlPoints[sourceIndex].RoutingTable.Keys.ToList()) sinkIndices.Remove(target.Index);

                while (sinkIndices.Count > 0)
                {
                    lock (ControlPoints)
                    {
                        //Console.Clear();
                        //Console.WriteLine("Constructing routing table...\t\n{0:F3}% of entire PathMover Completed!\t", 100.0 * ControlPoints.Values.Sum(cp => cp.RoutingTable.Count) / ControlPoints.Count / (ControlPoints.Count - 1));
                    }

                    var sinkIndex = sinkIndices.First();
                    var path = dijkstra.ShortestPath(sourceIndex, sinkIndex);
                    if (path.Count > 0)
                    {
                        path.Add(sourceIndex);
                        path.Reverse();
                        for (int i = 0; i < path.Count - 1; i++)
                        {
                            for (int j = i + 1; j < path.Count; j++)
                                lock (ControlPoints)
                                    controlPoints[path[i]].RoutingTable[controlPoints[path[j]]] = controlPoints[path[i + 1]];
                            sinkIndices.Remove(path[i + 1]);
                        }
                    }
                    else
                    {
                        lock (ControlPoints) controlPoints[sourceIndex].RoutingTable[controlPoints[sinkIndex]] = null;
                        sinkIndices.Remove(sinkIndex);
                    }
                }
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
            #endregion

            #region For Drawing
            public TransformGroup TransformGroup { get; } = new TransformGroup();

            private bool _showTag = true;
            private Canvas _drawing = null;
            public Canvas Drawing { get { if (_drawing == null) UpdDrawing(); return _drawing; } }
            public bool ShowTag { get { return _showTag; } set { if (_showTag != value) { _showTag = value; UpdDrawing(); } } }
            public void UpdDrawing(DateTime? clockTime = null)
            {
                _drawing = new Canvas();
                foreach (var cp in ControlPoints.Values) { cp.ShowTag = ShowTag; _drawing.Children.Add(cp.Drawing); }
                foreach (var path in Paths.Values) { path.ShowTag = ShowTag; _drawing.Children.Add(path.Drawing); }
                _drawing.RenderTransform = TransformGroup;
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
                if (Vehicle.Targets.Count == 0) throw new Exception("Vechile must have at least one target that is different from the departing point.");
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
                foreach (var path in At.PathsIn.Select(p => This.Paths[p]))
                {
                    Execute(path.UpdToExit(path, ToArrive));
                    if (path.Config.CrossHatched)
                    {
                        foreach (var prev in path.Config.Start.PathsIn.Select(p => This.Paths[p]))
                            Execute(prev.UpdToExit(path, ToArrive));
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
            public override void Invoke()
            {
                var nLockedByPaths = This.Paths.Values.Count(p => p.LockedByPaths);

                //var paths_lockedByPaths = This.Paths.Values.Where(p => p.LockedByPaths).ToList();
                //var paths_locked = This.Paths.Values.Where(p => p.VehiclesCompleted.Count == p.Occupancy && p.Occupancy > 0).ToList();
                //if (nLockedByPaths < paths_locked.Count) Console.WriteLine();

                var nOccupied = This.Paths.Values.Count(p => p.Occupancy > 0);
                if (nOccupied > 0 && nLockedByPaths == nOccupied)
                {
                    //string paths = "";
                    //foreach (var p in This.Paths.Values.Where(p => p.Occupancy > 0)) paths += string.Format("{0},", p);
                    //Log(string.Format("Deadlock Occurs at Path #{0}.", paths.Substring(0, paths.Length - 1)));
                    Execute(This.OnDeadlock.Select(e => e()));
                }
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
        #endregion

        #region Input Events - Getters
        public Event CallToDepart(IVehicle vehicle, ControlPoint at) { return new CallToDepartEvent { This = this, Vehicle = vehicle, At = at }; }
        public Event UpdToArrive(ControlPoint at, bool toArrive) { return new UpdToArriveEvent { This = this, At = at, ToArrive = toArrive }; }
        /// <summary>
        /// Reset the PathMover by removing all vehicles, and releasing locks caused by congested paths.
        /// </summary>
        public Event Reset() { return new ResetEvent { This = this }; }
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
                path.OnExit.Add(veh => new DepartEvent { This = this, Path = path });
            }
            foreach (var path in Paths.Values)
            {
                foreach (var prev in path.Config.Start.PathsIn.Select(p => Paths[p]))
                {
                    path.OnVacancyChg.Add(() => prev.UpdToEnter(path, path.Vacancy > 0));
                    InitEvents.Add(prev.UpdToEnter(path, path.Vacancy > 0));
                    InitEvents.Add(path.UpdToExit(path, true));
                    // cross-hatching
                    if (prev.Config.CrossHatched)
                        foreach (var prev2 in prev.Config.Start.PathsIn.Select(p => Paths[p]))
                        {
                            path.OnVacancyChg.Add(() => prev2.UpdToEnter(path, path.Vacancy > 0));
                            InitEvents.Add(prev2.UpdToEnter(path, path.Vacancy > 0));
                        }
                    if (path.Config.CrossHatched)
                    {
                        InitEvents.Add(prev.UpdToExit(path, true));
                    }
                    path.OnLockedByPaths.Add(() => new ChkDeadlockEvent { This = this });
                }
            }
        }

        public override void WarmedUp(DateTime clockTime)
        {
            foreach (var path in Paths.Values) path.WarmedUp(clockTime);
            HCounter_Departing.WarmedUp(clockTime);
            HCounter_Travelling.WarmedUp(clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime = default(DateTime?))
        {
            foreach (var path in Paths.Values) path.WriteToConsole();
        }

        #region For Drawing
        private DateTime? _timestamp = null;
        private Canvas _drawing = null;

        public TransformGroup TransformGroup { get { return Config.TransformGroup; } }
        public Canvas Drawing { get { if (_drawing == null) { InitDrawing(); UpdDrawing(); } return _drawing; } }
        public bool ShowTag
        {
            get { return Config.ShowTag; }
            set { if (Config.ShowTag != value) { Config.ShowTag = value; UpdDrawing(_timestamp); } }
        }
        private void InitDrawing()
        {
            _drawing = new Canvas();
            foreach (var cp in Config.ControlPoints.Values) _drawing.Children.Add(cp.Drawing);
            foreach (var path in Paths.Values) _drawing.Children.Add(path.Drawing);
            _drawing.RenderTransform = Config.TransformGroup;
        }
        public void UpdDrawing(DateTime? clockTime = null)
        {
            foreach (var path in Paths.Values)
            {
                path.ShowTag = ShowTag;
                path.UpdDrawing(clockTime);
            }
        }
        #endregion

    }
}
