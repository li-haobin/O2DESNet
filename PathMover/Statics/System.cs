using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    class System
    {
        public List<Path> Paths { get; private set; }
        public List<ControlPoint> ControlPoints { get; private set; }
        public List<Vehicle> Vehicles { get; private set; }

        public System()
        {
            Paths = new List<Path>();
            ControlPoints = new List<ControlPoint>();
            Vehicles = new List<Vehicle>();
        }

        /// <summary>
        /// Create and return a new path
        /// </summary>
        public Path CreatePath(double length, double maxSpeed = double.PositiveInfinity, Direction direction = Direction.TwoWay)
        {
            var path = new Path(Paths.Count, length, maxSpeed, direction);
            Paths.Add(path);
            return path;
        }
        /// <summary>
        /// Create and return a new control point
        /// </summary>
        public ControlPoint CreateControlPoint(Path path, double position)
        {
            var controlPoint = new ControlPoint(ControlPoints.Count);
            path.Add(controlPoint, position);
            ControlPoints.Add(controlPoint);
            return controlPoint;
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
        public void ConstructRouteTables()
        {
            var edges = Paths.SelectMany(path => GetEdges(path)).ToArray();
            foreach (var cp in ControlPoints) cp.RouteTable = new Dictionary<ControlPoint, ControlPoint>();
            var incompleteSet = ControlPoints.ToList();
            while (incompleteSet.Count > 0)
            {
                ConstructRouteTables(incompleteSet.First().Id + 1, edges);
                incompleteSet.RemoveAll(cp => cp.RouteTable.Count == ControlPoints.Count - 1);
            }
        }
        private void ConstructRouteTables(int sourceIndex, Dijkstra.Edge[] edges)
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
                    if (!ControlPoints[parent - 1].RouteTable.ContainsKey(ControlPoints[target - 1]))
                        ControlPoints[parent - 1].RouteTable.Add(ControlPoints[target - 1], ControlPoints[current - 1]);
                    current = parent;
                }
            }
        }
    }
}
