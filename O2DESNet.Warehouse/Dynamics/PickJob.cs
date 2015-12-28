using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    public class PickJob
    {
        public SKU item { get; set; }
        public CPRack rack { get; set; }
        public int quantity { get; set; }

        public PickJob(SKU sku, CPRack loc, int qty = 1)
        {
            item = sku; rack = loc; quantity = qty;
        }
    }
}
