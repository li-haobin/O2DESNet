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
        public ControlPoint CurLocation { get; set; }
        public PickerType Type { get; private set; }
        public List<PickJob> PickList { get; set; }
        public Dictionary<SKU, int> Items { get; set; }

        public Picker(PickerType type)
        {
            CurLocation = null;
            Type = type;
            PickList = new List<PickJob>();
            Items = new Dictionary<SKU, int>();
        }

        // All time in seconds
        public double GetTravelTime(ControlPoint destination)
        {
            return Type.GetNextTravelTime(CurLocation, destination);
        }
        public TimeSpan GetPickingTime()
        {
            return Type.GetNextPickingTime();
        }
        public void PickNextItem()
        {
            var pickJob = PickList.First();
            if (CurLocation != pickJob.rack.OnShelf.BaseCP) throw new Exception("ERROR! Wrong location, halt pick");

            pickJob.item.PickFromRack(pickJob.rack, pickJob.quantity);

            if (!Items.ContainsKey(pickJob.item)) Items.Add(pickJob.item, 1);
            else Items[pickJob.item] += pickJob.quantity;

            PickList.RemoveAt(0);
        }
    }
}
