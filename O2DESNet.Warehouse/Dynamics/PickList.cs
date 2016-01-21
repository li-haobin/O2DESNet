using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    public class PickList
    {
        private static int _count = 0;
        public int pickListID { get; private set; }
        public List<PickJob> pickJobs { get; set; }
        public List<Order> orders { get; set; } // order-based
        public OrderBatch orderBatch { get; set; } // item-based
        public Picker picker { get; set; }

        public PickList()
        {
            pickListID = ++_count;
            pickJobs = new List<PickJob>();
            orders = new List<Order>();
            orderBatch = null;
            picker = null;
        }
    }
}
