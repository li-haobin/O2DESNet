using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.Statics
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

        #region Layout Builder
        /// <summary>
        /// Create and return a new aisle
        /// </summary>
        public PathAisle CreateAisle(double length, double maxSpeed = double.PositiveInfinity, Direction direction = Direction.TwoWay)
        {
            var aisle = new PathAisle(length, maxSpeed, direction);
            Paths.Add(aisle);
            return aisle;
        }
        /// <summary>
        /// Create and return a new row, connected to aisle(s)
        /// </summary>
        public PathRow CreateRow(double length, PathAisle aisleIn, double inPos, PathAisle aisleOut = null, double outPos = double.NegativeInfinity,
            double maxSpeed = double.PositiveInfinity, Direction direction = Direction.TwoWay)
        {
            var row = new PathRow(length, aisleIn, aisleOut, maxSpeed, direction);
            Paths.Add(row);
            Connect(row, aisleIn, 0, inPos);
            if (aisleOut != null)
                if (!double.IsNegativeInfinity(outPos))
                    Connect(row, aisleOut, row.Length, outPos);
                else
                    throw new Exception("Specify aisleOut position");

            return row;
        }
        /// <summary>
        /// Create and return a new shelf, connected to a row
        /// </summary>
        public PathShelf CreateShelf(double height, PathRow row, double pos,
            double maxSpeed = double.PositiveInfinity, Direction direction = Direction.TwoWay)
        {
            var shelf = new PathShelf(height, row, maxSpeed, direction);
            Paths.Add(shelf);
            Connect(shelf, row, 0, pos);
            return shelf;
        }
        /// <summary>
        /// Create and return a new rack, on a shelf. Optional SKUs on rack.
        /// </summary>
        public CPRack CreateRack(PathShelf shelf, double position, List<SKU> SKUs = null)
        {
            var rack = (CPRack)CreateControlPoint(shelf, position);
            rack.InitializeRack();

            if(SKUs != null)
                foreach(var s in SKUs)
                {
                    shelf.SKUs.Add(s, rack);
                    AddToRack(s, rack);
                }

            return rack;
        }
        /// <summary>
        /// Add SKU into a Rack
        /// </summary>
        public void AddToRack(SKU _sku, CPRack rack)
        {
            _sku.Racks.Add(rack);
            rack.SKUs.Add(_sku);
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

        public void AddVehicles(VehicleType vehicleType, int number)
        {
            if (!NumsVehicles.ContainsKey(vehicleType)) NumsVehicles.Add(vehicleType, 0);
            NumsVehicles[vehicleType] += number;
        }
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
                ConstructRoutingTables(incompleteSet.First().Id, edges);
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
                var from = path.ControlPoints[i].Id;
                var to = path.ControlPoints[i + 1].Id;
                if (path.Direction != Direction.Backward) edges.Add(new Dijkstra.Edge(from, to, length));
                if (path.Direction != Direction.Forward) edges.Add(new Dijkstra.Edge(to, from, length));
            }
            return edges;
        }
        #endregion
    }
}
