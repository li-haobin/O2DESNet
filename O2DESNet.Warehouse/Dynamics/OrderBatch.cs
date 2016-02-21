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
        public static int _count = 0;

        public int BatchID { get; private set; }
        public List<Order> Orders { get; set; }
        public List<PickList> PickLists { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime CompletionTime { get; set; } // Send to sorting

        public OrderBatch(List<Order> orders)
        {
            StartTime = DateTime.MinValue;
            CompletionTime = DateTime.MinValue;
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
