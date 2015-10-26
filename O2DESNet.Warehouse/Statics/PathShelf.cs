using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.Statics
{
    public class PathShelf : Path
    {
        public string Shelf_ID { get; set; }
        public PathRow Row { get; set; }
        public Dictionary<SKU, CPRack> SKUs { get; set; }

        public PathShelf(double height, PathRow row, double maxSpeed, Direction direction)
            : base(height, maxSpeed, direction)
        {
            Row = row;
            if (Row == null)
                throw new Exception("Shelf must be connected to a row");
            else
                Row.Shelves.Add(this);

            SKUs = new Dictionary<SKU, CPRack>();
        }

    }
}
