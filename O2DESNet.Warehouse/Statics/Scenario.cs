using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.Statics
{
    public class Scenario
    {
        /// <summary>
        /// Parent Classes for Dijkstra
        /// </summary>
        public List<Path> Paths { get; private set; }
        public List<ControlPoint> ControlPoints { get; private set; }
        /// <summary>
        /// Numbers of vehicles of each type
        /// </summary>
        public Dictionary<PickerType, int> NumPickers { get; private set; }
        /// <summary>
        /// Lists which are useful
        /// </summary>
        public List<PathAisle> Aisles { get; private set; }
        public List<PathRow> Rows { get; private set; }
        public List<PathShelf> Shelves { get; private set; }
        public List<CPRack> Racks { get; private set; }
        public HashSet<SKU> SKUs { get; private set; }

        public Scenario()
        {
            Paths = new List<Path>();
            ControlPoints = new List<ControlPoint>();
            NumPickers = new Dictionary<PickerType, int>();
            Aisles = new List<PathAisle>();
            Rows = new List<PathRow>();
            Shelves = new List<PathShelf>();
            Racks = new List<CPRack>();
            SKUs = new HashSet<SKU>();
        }

        #region View Layout
        public void ViewAll()
        {
            ViewAisles();
            ViewRows();
            ViewShelves();
            ViewRacks();
            ViewSKUs();
        }
        public void ViewAisles()
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("                Aisles                ");
            Console.WriteLine("--------------------------------------");

            foreach (var a in Aisles)
            {
                Console.WriteLine("Aisle {0}\tLength {1}", a.Aisle_ID, a.Length);
                Console.WriteLine("Row_ID\tLength");
                foreach (var r in a.Rows)
                {
                    Console.WriteLine("{0}\t{1}", r.Row_ID, r.Length);
                }

            }
        }
        public void ViewRows()
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("                 Rows                 ");
            Console.WriteLine("--------------------------------------");

            foreach (var r in Rows)
            {
                Console.WriteLine("Row {0}\tLength {1}", r.Row_ID, r.Length);
                Console.WriteLine("In {0}\tOut {1}", r.AisleIn.Aisle_ID, ((r.AisleOut != null) ? r.AisleOut.Aisle_ID : "NIL"));
                Console.WriteLine("Shelf_ID\tHeight");
                foreach (var s in r.Shelves)
                {
                    Console.WriteLine("{0}\t{1}", s.Shelf_ID, s.Length);
                }

            }
        }
        public void ViewShelves()
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("                Shelves               ");
            Console.WriteLine("--------------------------------------");

            foreach (var s in Shelves)
            {
                Console.WriteLine("Shelf {0}\tHeight {1}\tOnRow {2}", s.Shelf_ID, s.Length, s.Row.Row_ID);
                foreach (var r in s.Racks)
                {
                    Console.WriteLine("{0}", r.Rack_ID);
                }

            }
        }
        public void ViewRacks()
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("                 Racks                ");
            Console.WriteLine("--------------------------------------");

            foreach (var r in Racks)
            {
                Console.WriteLine("Rack {0}", r.Rack_ID);
                foreach (var s in r.SKUs)
                {
                    Console.WriteLine("{0}", s.SKU_ID);
                }

            }
        }
        public void ViewSKUs()
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("                 SKUs                 ");
            Console.WriteLine("--------------------------------------");

            Console.WriteLine("SKU_ID\tDescription\tRacks");
            var SKUList = SKUs.ToList();
            foreach (var s in SKUList)
            {
                Console.Write("{0}\t{1}\t", s.SKU_ID, s.Description);
                foreach (var r in s.Racks)
                    Console.Write("{0} ", r.Rack_ID);
                Console.WriteLine("");
            }

        }
        #endregion

        #region Layout Builder
        /// <summary>
        /// Create and return a new aisle
        /// </summary>
        public PathAisle CreateAisle(string aisle_ID, double length, double maxSpeed = double.PositiveInfinity, Direction direction = Direction.TwoWay)
        {
            var aisle = new PathAisle(aisle_ID, length, maxSpeed, direction);
            Paths.Add(aisle);
            Aisles.Add(aisle);
            return aisle;
        }
        /// <summary>
        /// Create and return a new row, connected to aisle(s)
        /// </summary>
        public PathRow CreateRow(string row_ID, double length, PathAisle aisleIn, double inPos, PathAisle aisleOut = null, double outPos = double.NegativeInfinity,
            double maxSpeed = double.PositiveInfinity, Direction direction = Direction.TwoWay)
        {
            var row = new PathRow(row_ID, length, aisleIn, aisleOut, maxSpeed, direction);
            Paths.Add(row);
            Rows.Add(row);
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
        public PathShelf CreateShelf(string shelf_ID, double height, PathRow row, double pos,
            double maxSpeed = double.PositiveInfinity, Direction direction = Direction.TwoWay)
        {
            var shelf = new PathShelf(shelf_ID, height, row, maxSpeed, direction);
            Paths.Add(shelf);
            Shelves.Add(shelf);
            Connect(shelf, row, 0, pos);
            return shelf;
        }
        /// <summary>
        /// Create and return a new rack, on a shelf. Optional SKUs on rack.
        /// </summary>
        public CPRack CreateRack(string rack_ID, PathShelf shelf, double position, List<SKU> SKUs = null)
        {
            var rack = new CPRack(rack_ID, shelf);
            shelf.Add(rack, position);
            ControlPoints.Add(rack);
            Racks.Add(rack);

            if (SKUs != null)
                foreach (var s in SKUs)
                {
                    AddToRack(s, rack);
                }

            return rack;
        }
        /// <summary>
        /// Add SKU into a Rack
        /// </summary>
        public void AddToRack(SKU _sku, CPRack rack)
        {
            if (!SKUs.Contains(_sku)) SKUs.Add(_sku);
            _sku.Racks.Add(rack);
            rack.SKUs.Add(_sku);
            rack.OnShelf.SKUs.Add(_sku, rack);
        }
        public void AddPickers(PickerType pickerType, int quantity)
        {
            if (!NumPickers.ContainsKey(pickerType)) NumPickers.Add(pickerType, 0);
            NumPickers[pickerType] += quantity;
        }

        // Consider making private the 3 methods below:
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
        #endregion

        #region For Static Routing (Distance-Based), Using Dijkstra
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
