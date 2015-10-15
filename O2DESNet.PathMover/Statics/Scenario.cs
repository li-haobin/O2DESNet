using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Statics
{
   public class Scenario
    {
        public List<Path> Paths { get; private set; }
        public List<ControlPoint> ControlPoints { get; private set; }
        /// <summary>
        /// Numbers of vehicles of each type
        /// </summary>
        public Dictionary<VehicleType, int> NumsVehicles { get; private set; }

        public Scenario()
        {
            Paths = new List<Path>();
            ControlPoints = new List<ControlPoint>();
            NumsVehicles = new Dictionary<VehicleType, int>();
        }

        #region Path Mover Builder
        /// <summary>
        /// Create and return a new path
        /// </summary>
        public Path CreatePath(double length, double maxSpeed = double.PositiveInfinity, Direction direction = Direction.TwoWay)
        {
            var path = new Path(length, maxSpeed, direction);
            Paths.Add(path);
            return path;
        }
        /// <summary>
        /// Create and return a new control point
        /// </summary>
        public ControlPoint CreateControlPoint(Path path, double position)
        {
            var controlPoint = new ControlPoint();
            path.Add(controlPoint, position);
            ControlPoints.Add(controlPoint);
            return controlPoint;
        }
        public void AddVehicles(VehicleType vehicleType, int number)
        {
            if (!NumsVehicles.ContainsKey(vehicleType)) NumsVehicles.Add(vehicleType, 0);
            NumsVehicles[vehicleType] += number;
        }
        /// <summary>
        /// Connect two paths at specified positions
        /// </summary>
        public void Connect(Path path_0, Path path_1, double position_0, double position_1)
        {
            var controlPoint = CreateControlPoint(path_0, position_0);
            path_1.Add(controlPoint, position_1);
        }
        /// <summary>
        /// Connect the end of path_0 to the start of path_1
        /// </summary>
        public void Connect(Path path_0, Path path_1) { Connect(path_0, path_1, path_0.Length, 0); }
        #endregion

        #region For Static Routing (Distance-Based)
        public void Initialize()
        {
            ConstructRoutingTables();
            ConstructPathingTables();
        }        
        private void ConstructRoutingTables()
        {
            foreach (var cp in ControlPoints) cp.RoutingTable = new Dictionary<ControlPoint, ControlPoint>();
            var incompleteSet = ControlPoints.ToList();
            var edges = Paths.SelectMany(path => GetEdges(path)).ToArray();            
            while (incompleteSet.Count > 0)
            {
                ConstructRoutingTables(incompleteSet.First().Id + 1, edges);
                incompleteSet.RemoveAll(cp => cp.RoutingTable.Count == ControlPoints.Count - 1);
            }
        }
        private void ConstructRoutingTables(int sourceIndex, Dijkstra.Edge[] edges)
        {
            var edgeList = edges.ToList();
            edgeList.Add(new Dijkstra.Edge(0, sourceIndex, 0)); // set the source
            var dijkstra = new Dijkstra(edgeList.ToArray());
            var parents = dijkstra.Parents;
            for (int target = 1; target < parents.Length; target++)
            {
                var current = target;
                while (current != sourceIndex)
                {
                    var parent = parents[current];
                    if (!ControlPoints[parent - 1].RoutingTable.ContainsKey(ControlPoints[target - 1]))
                        ControlPoints[parent - 1].RoutingTable.Add(ControlPoints[target - 1], ControlPoints[current - 1]);
                    current = parent;
                }
            }
        }
        private void ConstructPathingTables()
        {
            foreach (var cp in ControlPoints) cp.PathingTable = new Dictionary<ControlPoint, Path>();
            foreach (var path in Paths)
            {
                // assume same pair of control points are connected only by one path
                if (path.Direction != Direction.Backward)
                    for (int i = 0; i < path.ControlPoints.Count - 1; i++)
                        path.ControlPoints[i].PathingTable.Add(path.ControlPoints[i + 1], path);
                if (path.Direction != Direction.Forward)
                    for (int i = path.ControlPoints.Count - 1; i > 0; i--)
                        path.ControlPoints[i].PathingTable.Add(path.ControlPoints[i - 1], path);
            }
        }
        private List<Dijkstra.Edge> GetEdges(Path path)
        {
            var edges = new List<Dijkstra.Edge>();
            for (int i = 0; i < path.ControlPoints.Count - 1; i++)
            {
                var length = path.ControlPoints[i + 1].Positions[path] - path.ControlPoints[i].Positions[path];
                var from = path.ControlPoints[i].Id + 1;
                var to = path.ControlPoints[i + 1].Id + 1;
                if (path.Direction != Direction.Backward) edges.Add(new Dijkstra.Edge(from, to, length));
                if (path.Direction != Direction.Forward) edges.Add(new Dijkstra.Edge(to, from, length));
            }
            return edges;
        }
        #endregion
    }
}
