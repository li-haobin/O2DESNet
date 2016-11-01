using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;
using O2DESNet.Optimizer;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Drawing;
using System.Drawing.Imaging;
using O2DESNet.SVGRenderer;
using System.IO;

namespace O2DESNet
{
    public class PathMover : Component<PathMover.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            private static int _count = 0;
            public int Index { get; private set; } = _count++;
            public List<Path.Statics> Paths { get; private set; }
            public List<ControlPoint.Statics> ControlPoints { get; private set; }
            public string RoutingTablesFile { get; set; } = null;

            public Statics()
            {
                Paths = new List<Path.Statics>();
                ControlPoints = new List<ControlPoint.Statics>();
            }

            #region Path Mover Builder

            /// <summary>
            /// Create and return a new path
            /// </summary>
            public Path.Statics CreatePath(double length, double fullSpeed, int capacity = int.MaxValue, Path.Direction direction = Path.Direction.TwoWay)
            {
                CheckInitialized();
                var path = new Path.Statics(this)
                {
                    Length = length,
                    FullSpeed = fullSpeed,
                    Capacity = capacity,
                    Direction = direction,
                };
                Paths.Add(path);
                return path;
            }

            /// <summary>
            /// Create and return a new control point
            /// </summary>
            public ControlPoint.Statics CreateControlPoint(Path.Statics path, double position)
            {
                CheckInitialized();
                var controlPoint = new ControlPoint.Statics(this);
                path.Add(controlPoint, position);
                ControlPoints.Add(controlPoint);
                return controlPoint;
            }

            /// <summary>
            /// Connect two paths at specified positions
            /// </summary>
            public void Connect(Path.Statics path_0, Path.Statics path_1, double position_0, double position_1)
            {
                CheckInitialized();
                path_1.Add(CreateControlPoint(path_0, position_0), position_1);
            }

            /// <summary>
            /// Connect the Path to the Control Point at specific positions
            /// </summary>
            public void Connect(Path.Statics path, double position, ControlPoint.Statics controlPoint)
            {
                CheckInitialized();
                if (controlPoint.Positions.ContainsKey(path)) throw new StaticsBuildException("The Control Point exists on the Path.");
                path.Add(controlPoint, position);
            }

            /// <summary>
            /// Connect the end of path_0 to the start of path_1
            /// </summary>
            public void Connect(Path.Statics path_0, Path.Statics path_1) { Connect(path_0, path_1, path_0.Length, 0); }

            private void CheckInitialized()
            {
                if (_initialized) throw new StaticsBuildException("PathMover cannot be modified after initialization.");
            }
            #endregion

            #region For Static Routing (Distance-Based)
            private bool _initialized = false;
            internal void Initialize()
            {
                if (!_initialized)
                {
                    ConstructRoutingTables();
                    ConstructPathingTables();
                    ConstructConflictTables();
                    _initialized = true;
                }
            }
            public void OutputRoutingTables()
            {
                if (RoutingTablesFile == null) return;
                if (!_initialized) Initialize();
                var str = string.Join(";", ControlPoints.Select(cp => string.Format("{0}.{1}", cp.Index, string.Join(",", cp.RoutingTable.Where(i => i.Value != null).Select(i => string.Format("{0}:{1}", i.Key.Index, i.Value.Index))))));
                using (StreamWriter sw = new StreamWriter(RoutingTablesFile)) sw.Write(str);
            }
            private int _nSources;
            private void ConstructRoutingTables()
            {
                if (RoutingTablesFile != null)
                {
                    try
                    {
                        var str = File.ReadAllText(RoutingTablesFile);
                        foreach (var rt in str.Split(';'))
                        {
                            var ln = rt.Split('.');
                            int index = Convert.ToInt32(ln[0]);
                            if (ln[1].Length > 0) ControlPoints[index].RoutingTable = ln[1].Split(',').Select(r => r.Split(':')).ToDictionary(r => ControlPoints[Convert.ToInt32(r[0])], r => ControlPoints[Convert.ToInt32(r[1])]);
                            else ControlPoints[index].RoutingTable = new Dictionary<ControlPoint.Statics, ControlPoint.Statics>();
                            foreach (var cp in ControlPoints)
                                if (cp != ControlPoints[index] && !ControlPoints[index].RoutingTable.ContainsKey(cp))
                                    ControlPoints[index].RoutingTable.Add(cp, null);
                        }
                        return;
                    }
                    catch { }
                }
                foreach (var cp in ControlPoints) cp.RoutingTable = new Dictionary<ControlPoint.Statics, ControlPoint.Statics>();
                var incompleteSet = ControlPoints.ToList();
                var edges = Paths.SelectMany(path => GetEdges(path)).ToList();
                while (incompleteSet.Count > 0)
                {
                    _nSources = incompleteSet.Count;
                    ConstructRoutingTables(incompleteSet.First().Index, edges);
                    incompleteSet.RemoveAll(cp => cp.RoutingTable.Count == ControlPoints.Count - 1);                    
                }
            }
            private void ConstructRoutingTables(int sourceIndex, List<Tuple<int, int, double>> edges)
            {
                var edgeList = edges.ToList();
                var dijkstra = new Dijkstra(edges);

                var sinkIndices = new HashSet<int>(ControlPoints.Select(cp => cp.Index));
                sinkIndices.Remove(sourceIndex);
                foreach (var target in ControlPoints[sourceIndex].RoutingTable.Keys) sinkIndices.Remove(target.Index);

                while (sinkIndices.Count > 0)
                {
                    Console.Clear();
                    Console.WriteLine("Construct routing table, {0} sources {1} sinks remaining...", _nSources, sinkIndices.Count);

                    var sinkIndex = sinkIndices.First();
                    var path = dijkstra.ShortestPath(sourceIndex, sinkIndex);
                    if (path.Count > 0)
                    {
                        path.Add(sourceIndex);
                        path.Reverse();
                        for (int i = 0; i < path.Count - 1; i++)
                        {
                            for (int j = i + 1; j < path.Count; j++)
                                ControlPoints[path[i]].RoutingTable[ControlPoints[path[j]]] = ControlPoints[path[i + 1]];
                            sinkIndices.Remove(path[i + 1]);
                        }
                    }
                    else
                    {
                        ControlPoints[sourceIndex].RoutingTable[ControlPoints[sinkIndex]] = null;
                        sinkIndices.Remove(sinkIndex);
                    }
                }
            }
            private void ConstructPathingTables()
            {
                foreach (var cp in ControlPoints) cp.PathingTable = new Dictionary<ControlPoint.Statics, Path.Statics>();
                foreach (var path in Paths)
                {
                    // assume same pair of control points are connected only by one path
                    if (path.Direction != Path.Direction.Backward)
                        for (int i = 0; i < path.ControlPoints.Count - 1; i++)
                            path.ControlPoints[i].PathingTable.Add(path.ControlPoints[i + 1], path);
                    if (path.Direction != Path.Direction.Forward)
                        for (int i = path.ControlPoints.Count - 1; i > 0; i--)
                            path.ControlPoints[i].PathingTable.Add(path.ControlPoints[i - 1], path);
                }
            }
            protected virtual void ConstructConflictTables()
            {
                foreach (var p1 in Paths)
                    foreach (var p2 in p1.ControlPoints.SelectMany(cp => cp.Positions.Keys).Distinct())
                        if (p2 != p1) p1.AddConflict(p2);
            }
            /// <summary>
            /// For constructing the graph
            /// </summary>
            private List<Tuple<int, int, double>> GetEdges(Path.Statics path)
            {
                var edges = new List<Tuple<int, int, double>>();
                for (int i = 0; i < path.ControlPoints.Count - 1; i++)
                {
                    var length = path.ControlPoints[i + 1].Positions[path] - path.ControlPoints[i].Positions[path];
                    var from = path.ControlPoints[i].Index;
                    var to = path.ControlPoints[i + 1].Index;
                    if (path.Direction != Path.Direction.Backward) edges.Add(new Tuple<int, int, double>(from, to, length));
                    if (path.Direction != Path.Direction.Forward) edges.Add(new Tuple<int, int, double>(to, from, length));
                }
                return edges;
            }
            #endregion

            #region SVG Output
            public virtual Group SVG(double x = 0, double y = 0, double degree = 0)
            {
                return new Group(id: "pm#" + Index, x: x, y: y, rotate: degree,
                    content: Paths.Select(path => path.SVG()).Concat(ControlPoints.Select(cp => cp.SVG())));
            }

            /// <summary>
            /// Including arrows, styles
            /// </summary>
            public static Definition SVGDefs
            {
                get
                {
                    var defs = new Definition();
                    defs.Add(Path.Statics.SVGDefs.Elements());
                    defs.Add(ControlPoint.Statics.SVGDefs.Elements());
                    defs.Add(Vehicle.Statics.SVGDefs.Elements());
                    return defs;
                }
            }
            #endregion
        }
        #endregion

        #region Sub-Components
        //internal Server<TScenario, TStatus, TLoad> H_Server { get; private set; }
        //internal Server<TScenario, TStatus, TLoad> R_Server { get; private set; }
        #endregion

        #region Dynamics
        public Dictionary<ControlPoint.Statics, ControlPoint> ControlPoints { get; private set; }
        public Dictionary<Path.Statics, Path> Paths { get; private set; }
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
        public class StaticsBuildException : Exception
        {
            public StaticsBuildException(string msg) : base(string.Format("PathMover Build Exception: {0}", msg)) { }
        }
        #endregion

        public PathMover(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "PathMover";

            Config.Initialize();
            ControlPoints = Config.ControlPoints.ToDictionary(cfg => cfg, cfg => new ControlPoint(cfg, DefaultRS.Next(), string.Format("CP${0}", cfg.Index)) { PathMover = this });
            Paths = Config.Paths.ToDictionary(cfg => cfg, cfg => new Path(cfg, DefaultRS.Next(), string.Format("PATH${0}", cfg.Index)) { PathMover = this });
            Vehicles = new HashSet<Vehicle>();

            // Attach Path Segments (FIFOServers) to Control Points
            foreach (var path in Paths.Values)
            {
                for (int i = 0; i < path.ControlPoints.Length - 1; i++)
                {
                    path.ControlPoints[i].OutgoingSegments.Add(path.ForwardSegments[i]);
                    path.ControlPoints[i].IncomingSegments.Add(path.BackwardSegments[i]);
                    path.ControlPoints[i + 1].IncomingSegments.Add(path.ForwardSegments[i]);
                    path.ControlPoints[i + 1].OutgoingSegments.Add(path.BackwardSegments[i]);
                }
            }
        }

        public override void WarmedUp(DateTime clockTime)
        {
            foreach (var path in Paths.Values) path.WarmedUp(clockTime);
            foreach (var cp in ControlPoints.Values) cp.WarmedUp(clockTime);
            StartTime = clockTime;
            LastUpdateTime = clockTime;

            foreach (var veh in Vehicles)
            {
                veh.ResetAnchors();
                veh.ResetStateHistory();
            }                
            
            foreach (var cp in ControlPoints.Values.Where(cp => cp.At != null))
                cp.At.LogAnchor(0, cp.At.Segment.Path.Config, cp.Config.Positions[cp.At.Segment.Path.Config] / cp.At.Segment.Path.Config.Length);
            foreach (var seg in Paths.Values.SelectMany(path => path.ForwardSegments.Concat(path.BackwardSegments))) seg.LogAnchors(null, clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime)
        {
            Console.WriteLine("=== Paths ===");
            foreach (var path in Paths.Values) path.WriteToConsole();
            Console.WriteLine();

            Console.WriteLine("=== Vehicles ===");
            foreach (var veh in Vehicles) veh.WriteToConsole(clockTime);
            Console.WriteLine();

            Console.WriteLine("---------------------------");
            Console.WriteLine("Remarks:");
            Console.WriteLine("! : Delayed by slow moving ahead");
            Console.WriteLine("!!: Completely stopped due zero vacancy ahead.");
        }

        #region SVG Output
        public virtual Group SVG(double x=0, double y=0, double rotate=0)
        {
            var g = new Group("pm", x: x, y: y, rotate: rotate);
            g.Add(Config.SVG());
            foreach (var veh in VehiclesHistory) g.Add(veh.SVG());
            g.Add(new Group(x: 100, y: 100, rotate: 0, content: new Clock(StartTime, LastUpdateTime)));
            return g;
        }

        public virtual Definition SVGDefs
        {
            get
            {
                var defs = new Definition();
                defs.Add(Statics.SVGDefs.Elements());
                foreach (var vehCate in VehiclesHistory.Select(veh => veh.Category).Distinct()) defs.Add(vehCate.SVG());
                return defs;
            }
        }
        #endregion
    }
}
