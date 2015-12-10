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
        public List<PickJob> CompletedJob { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsIdle { get; set; }

        public Picker(PickerType type)
        {
            CurLocation = null;
            Type = type;
            PickList = new List<PickJob>();
            CompletedJob = new List<PickJob>();
        }

        // All time in seconds
        public TimeSpan GetTravelTime(ControlPoint destination)
        {
            return TimeSpan.FromSeconds(Type.GetNextTravelTime(CurLocation, destination));
        }
        public TimeSpan GetPickingTime()
        {
            return Type.GetNextPickingTime();
        }
        public void PickItem()
        {
            var pickJob = PickList.First();
            if (CurLocation != pickJob.rack.OnShelf.BaseCP) throw new Exception("ERROR! Wrong location, halt pick");

            pickJob.item.PickFromRack(pickJob.rack, pickJob.quantity);

            CompletedJob.Add(pickJob);

            PickList.RemoveAt(0);
        }

        public int GetNumCompletedPickJobs()
        {
            return CompletedJob.Count;
        }
        public TimeSpan GetTimeToCompleteJobs()
        {
            return EndTime - StartTime;
        }
    }
}
