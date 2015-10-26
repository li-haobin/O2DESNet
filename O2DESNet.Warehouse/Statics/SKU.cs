using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.Statics
{
    public class SKU
    {
        public string SKU_ID { get; private set; }
        public string Description { get; private set; }
        public List<CPRack> Racks { get; set; }

        public SKU(string sku_id, string description="")
        {
            SKU_ID = sku_id;
            Description = description;
            Racks = new List<CPRack>();
        }
    }
}
