using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Statics
{
    public class CPRack : ControlPoint
    {
        public string Rack_ID { get; set; }
        public PathShelf OnShelf { get; set; }
        public List<SKU> SKUs { get; set; }

        public CPRack(string rack_ID, PathShelf shelf) : base()
        {
            Rack_ID = rack_ID;
            OnShelf = shelf;
            if (shelf == null)
                throw new Exception("Rack must be connected to a shelf");
            else
                OnShelf.Racks.Add(this);

            SKUs = new List<SKU>();
        }
    }
}
