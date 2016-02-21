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
        public DateTime startPickTime { get; set; }
        public DateTime endPickTime { get; set; }

        public PickList()
        {
            pickListID = ++_count;
            pickJobs = new List<PickJob>();
            orders = new List<Order>();
            orderBatch = null;
            picker = null;
        }

        public void Add(PickJob pickjob)
        {
            pickJobs.Add(pickjob);
        }

        public PickJob First()
        {
            return pickJobs.First();
        }

        public void RemoveAt(int index)
        {
            pickJobs.RemoveAt(0);
        }

        public int ItemCount
        {
            get { return pickJobs.Count; }
        }

        public double GetUtilisation()
        {
            double utilisation;

            if (picker.Type.PickerType_ID == PicklistGenerator.A_PickerID ||
                picker.Type.PickerType_ID == PicklistGenerator.B_PickerID_SingleZone ||
                picker.Type.PickerType_ID == PicklistGenerator.B_PickerID_MultiZone ||
                picker.Type.PickerType_ID == PicklistGenerator.C_PickerID_SingleZone)
            {
                // Order-based
                utilisation = 1.0 * orders.Count / picker.Type.Capacity;
            }
            else
            {
                // Item-based
                utilisation = 1.0 * pickJobs.Count / picker.Type.Capacity;
            }

            return utilisation;
        }
    }
}
