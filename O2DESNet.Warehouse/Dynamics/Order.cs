using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    public class Order
    {
        public string Order_ID { get; set; }
        public List<SKU> items { get; set; }

        public Order(string id)
        {
            Order_ID = id;
            items = new List<SKU>();
        }
    }
}
