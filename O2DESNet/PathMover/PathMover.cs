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

namespace O2DESNet
{
    public class PathMover : Component<PathMover.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public List<Path.Statics> Paths { get; private set; }
            public List<ControlPoint.Statics> ControlPoints { get; private set; }

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

            private void CheckInitialized() {
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
                    _initialized = true;
                }
            }
            private void ConstructRoutingTables()
            {
                foreach (var cp in ControlPoints) cp.RoutingTable = new Dictionary<ControlPoint.Statics, ControlPoint.Statics>();
                var incompleteSet = ControlPoints.ToList();
                var edges = Paths.SelectMany(path => GetEdges(path)).ToList();
                while (incompleteSet.Count > 0)
                {
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
                    else {
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

            #region For Display

            public DenseVector GetCoord(ControlPoint.Statics cp, ref DenseVector towards)
            {
                var pos = cp.Positions.First();
                return LinearTool.SlipOnCurve(pos.Key.Coordinates, ref towards, pos.Value / pos.Key.Length);
            }

            internal protected virtual void InitDrawingParams(DrawingParams dParams)
            {
                dParams.Resize(Paths.SelectMany(p => p.Coordinates));
            }

            public Bitmap DrawToImage(DrawingParams dParams, bool init = true)
            {
                if (init) InitDrawingParams(dParams);
                Bitmap bitmap = new Bitmap(Convert.ToInt32(dParams.Width), Convert.ToInt32(dParams.Height), PixelFormat.Format32bppArgb);
                Draw(Graphics.FromImage(bitmap), dParams, init: false);
                return bitmap;
            }

            public void DrawToFile(string file, DrawingParams dParams)
            {
                InitDrawingParams(dParams);
                DrawToImage(dParams, init: false).Save(file, ImageFormat.Png);
            }

            public virtual void Draw(Graphics g, DrawingParams dParams, bool init = true)
            {
                if (init) InitDrawingParams(dParams);
                foreach (var path in Paths) DrawPath(g, path, dParams);
                foreach (var cp in ControlPoints) DrawControlPoint(g, cp, dParams);
            }

            private void DrawControlPoint(Graphics g, ControlPoint.Statics cp, DrawingParams dParams)
            {
                DenseVector towards = null;
                DenseVector coord = GetCoord(cp, ref towards);
                var tail = LinearTool.SlipByDistance(coord, coord + (towards - coord), dParams.ControlPointSize / 2);

                var pen = new Pen(dParams.ControlPointColor, dParams.ControlPointThickness);
                g.DrawLine(pen, dParams.GetPoint(LinearTool.Rotate(tail, coord, Math.PI / 4)), dParams.GetPoint(LinearTool.Rotate(tail, coord, -3 * Math.PI / 4)));
                g.DrawLine(pen, dParams.GetPoint(LinearTool.Rotate(tail, coord, -Math.PI / 4)), dParams.GetPoint(LinearTool.Rotate(tail, coord, 3 * Math.PI / 4)));
            }

            private void DrawPath(Graphics g, Path.Statics path, DrawingParams dParams)
            {
                if (path.Coordinates.Count == 0) return;
                var pen = dParams.PathStyle;
                path.Draw(g, dParams, pen, 0, 1);
                // draw arrows on path
                DenseVector vetex, tail, towards = null;
                if (path.Direction == Path.Direction.TwoWay || path.Direction == Path.Direction.Forward)
                {
                    vetex = LinearTool.SlipOnCurve(path.Coordinates, ref towards, 0.4);
                    tail = LinearTool.SlipByDistance(vetex, towards, -dParams.ArrowSize);
                    g.DrawLine(pen, dParams.GetPoint(vetex), dParams.GetPoint(LinearTool.Rotate(tail, vetex, dParams.ArrowAngle / 2)));
                    g.DrawLine(pen, dParams.GetPoint(vetex), dParams.GetPoint(LinearTool.Rotate(tail, vetex, -dParams.ArrowAngle / 2)));
                }
                if (path.Direction == Path.Direction.TwoWay || path.Direction == Path.Direction.Backward)
                {
                    vetex = LinearTool.SlipOnCurve(path.Coordinates, ref towards, 0.6);
                    tail = LinearTool.SlipByDistance(vetex, towards, dParams.ArrowSize);
                    g.DrawLine(pen, dParams.GetPoint(vetex), dParams.GetPoint(LinearTool.Rotate(tail, vetex, dParams.ArrowAngle / 2)));
                    g.DrawLine(pen, dParams.GetPoint(vetex), dParams.GetPoint(LinearTool.Rotate(tail, vetex, -dParams.ArrowAngle / 2)));
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
        }

        public override void WriteToConsole()
        {
            Console.WriteLine("=== Paths ===");
            foreach (var path in Paths.Values) path.WriteToConsole();
            Console.WriteLine();

            Console.WriteLine("=== Vehicles ===");
            foreach (var veh in Vehicles) veh.WriteToConsole();
            Console.WriteLine();

            Console.WriteLine("---------------------------");
            Console.WriteLine("Remarks:");
            Console.WriteLine("! : Delayed by slow moving ahead");
            Console.WriteLine("!!: Completely stopped due zero vacancy ahead.");
        }
    }
}
