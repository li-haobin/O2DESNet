using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    public class OrderBatch
    {
        private static int _count = 0;

        public int BatchID { get; private set; }
        public List<Order> Orders { get; set; }
        public List<PickList> PickLists { get; set; }

        public OrderBatch(List<Order> orders)
        {
            BatchID = ++_count;
            Orders = orders;
            PickLists = new List<PickList>();
        }

        public static int GetTotalNumBatches()
        {
            return _count;
        }
    }
}
