using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.Statics
{
    public class PathAisle : Path
    {
        public string Aisle_ID { get; set; }
        public List<PathRow> Rows { get; set; }

        public PathAisle(string aisle_ID, double length, double maxSpeed, Direction direction)
            : base(length, maxSpeed, direction)
        {
            Aisle_ID = aisle_ID;
            Rows = new List<PathRow>();
        }
    }
}
