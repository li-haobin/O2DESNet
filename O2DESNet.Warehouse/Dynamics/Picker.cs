using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    public class Picker
    {
        public ControlPoint CurLocation;
        public PickerType Type;
        public List<PickJob> PickList;
        public Dictionary<SKU, int> Items;

        public Picker(PickerType type)
        {
            CurLocation = null;
            Type = type;
            PickList = new List<PickJob>();
            Items = new Dictionary<SKU, int>();
        }

        // All time in seconds
        public double GetNextTravelTime(ControlPoint destination)
        {
            return Type.GetTravellingTime(CurLocation, destination);
        }
        public TimeSpan GetNextPickingTime()
        {
            return Type.GetPickingTime();
        }
        public void PickNextItem()
        {
            var pickJob = PickList.First();
            if (CurLocation != pickJob.location) throw new Exception("Wrong location, halt pick");

            pickJob.item.PickFromRack(pickJob.location, pickJob.quantity);

            if (!Items.ContainsKey(pickJob.item)) Items.Add(pickJob.item, 1);
            else Items[pickJob.item] += pickJob.quantity;

            PickList.RemoveAt(0);
        }
    }
}
