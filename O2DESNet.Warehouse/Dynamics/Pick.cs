using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    public class Pick
    {
        public SKU item;
        public CPRack location;
        public int quantity;
    }
}
