using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Statics
{
    [Serializable]
    public class PathShelf : Path
    {
        public string Shelf_ID { get; set; }
        public PathRow Row { get; set; }
        public List<CPRack> Racks { get; set; }
        public ControlPoint BaseCP { get; set; }
        public Dictionary<SKU, CPRack> SKUs { get; set; }

        public PathShelf(string shelf_ID, double height, PathRow row, double maxSpeed, Direction direction)
            : base(height, maxSpeed, direction)
        {
            _count--; //Exclude Shelf from Dijkstra : for performance
            Shelf_ID = shelf_ID;
            Row = row;
            if (Row == null)
                throw new Exception("Shelf must be connected to a row");
            else
                Row.Shelves.Add(this);

            Racks = new List<CPRack>();
            SKUs = new Dictionary<SKU, CPRack>();
        }

    }
}
