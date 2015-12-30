using O2DESNet.Warehouse.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace O2DESNet.Warehouse.Dynamics
{
    internal class Status
    {
        private Simulator _sim;



        // Possible to discriminate by PickerType
        public Dictionary<PickerType, int> TotalPickJobsCompleted { get; private set; }
        public Dictionary<PickerType, int> TotalPickListsCompleted { get; private set; }
        public Dictionary<PickerType, TimeSpan> TotalPickingTime { get; private set; }

        internal Status(Simulator simulation)
        {
            _sim = simulation;

            TotalPickJobsCompleted = new Dictionary<PickerType, int>();
            TotalPickListsCompleted = new Dictionary<PickerType, int>();
            TotalPickingTime = new Dictionary<PickerType, TimeSpan>();

            foreach(var type in _sim.Scenario.NumPickers)
            {
                TotalPickJobsCompleted.Add(type.Key, 0);
                TotalPickListsCompleted.Add(type.Key, 0);
                TotalPickingTime.Add(type.Key, TimeSpan.Zero);
            }

        }

        public void CaptureCompletedPickList(Picker picker)
        {
            _sim.Scenario.CompletedPickLists[picker.Type].Add(picker.CompletedJobs);
            TotalPickingTime[picker.Type] += picker.GetTimeToCompletePickList();
            TotalPickJobsCompleted[picker.Type] += picker.GetNumCompletedPickJobs();
            TotalPickListsCompleted[picker.Type]++;
        }

        public TimeSpan GetAveragePickListTime(PickerType type)
        {
            if (TotalPickListsCompleted[type] == 0) return TimeSpan.Zero;

            return TimeSpan.FromSeconds(TotalPickingTime[type].TotalSeconds / TotalPickListsCompleted[type]);
        }
    }
}
