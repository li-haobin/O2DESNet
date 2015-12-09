using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    public class SKU
    {
        public string SKU_ID { get; private set; }
        public string Description { get; private set; }
        public Dictionary<CPRack, int> Racks { get; set; } // This makes it dynamic actually...

        public SKU(string sku_id, string description="")
        {
            SKU_ID = sku_id;
            Description = description;
            Racks = new Dictionary<CPRack, int>();
        }
    }
}
