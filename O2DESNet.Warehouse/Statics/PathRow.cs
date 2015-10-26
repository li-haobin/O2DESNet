using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.Statics
{
    public class PathRow : Path
    {
        public string Row_ID { get; set; }
        public PathAisle AisleIn { get; private set; }
        public PathAisle AisleOut { get; private set; }
        public List<PathShelf> Shelves { get; set; }

        public PathRow(string row_ID, double length, PathAisle aisleIn, PathAisle aisleOut, double maxSpeed, Direction direction)
            : base(length, maxSpeed, direction)
        {
            Row_ID = row_ID;
            AisleIn = aisleIn;
            AisleOut = aisleOut;
            Shelves = new List<PathShelf>();

            if (AisleIn == null)
                throw new Exception("Row must be connected to at least one aisle");
            else
                AisleIn.Rows.Add(this);

            if (AisleOut != null) AisleOut.Rows.Add(this);
        }
    }
}
