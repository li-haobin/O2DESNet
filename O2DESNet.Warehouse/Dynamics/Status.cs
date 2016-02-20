using O2DESNet.Warehouse.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace O2DESNet.Warehouse.Dynamics
{
    [Serializable]
    internal class Status
    {
        private Simulator _sim;
        /// <summary>
        /// Simulator Start Time
        /// </summary>
        public DateTime StartTime { get; private set; }

        #region Order Picking and Picklist
        public Dictionary<PickerType, int> TotalPickJobsCompleted { get; private set; }
        public Dictionary<PickerType, int> TotalPickListsCompleted { get; private set; }
        public Dictionary<PickerType,int> MaxPickListSize { get; private set; }
        public Dictionary<PickerType, int> MinPickListSize { get; private set; }
        public Dictionary<PickerType, TimeSpan> TotalPickingTime { get; private set; }

        public int NumActivePickers { get; private set; }
        public int MaxActivePickers { get; private set; }
        public TimeSpan AreaPickerTime { get; private set; }
        public DateTime NumPickersJumpTime { get; private set; }
        #endregion

        #region Consolidator
        public List<int> OrderBatchesTotesCount { get; set; }
        public int NumItemsSorted { get; set; }
        public int MaxNumItemsSorted { get; set; }
        public int NumActiveSorters { get; private set; }
        public int MaxActiveSorters { get; private set; }
        #endregion

        internal Status(Simulator simulation)
        {
            _sim = simulation;

            TotalPickJobsCompleted = new Dictionary<PickerType, int>();
            TotalPickListsCompleted = new Dictionary<PickerType, int>();
            TotalPickingTime = new Dictionary<PickerType, TimeSpan>();
            MaxPickListSize = new Dictionary<PickerType, int>();
            MinPickListSize = new Dictionary<PickerType, int>();

            NumActivePickers = 0;
            MaxActivePickers = 0;
            AreaPickerTime = TimeSpan.Zero;
            NumPickersJumpTime = _sim.ClockTime;
            StartTime = _sim.ClockTime;

            foreach (var type in _sim.Scenario.NumPickers)
            {
                TotalPickJobsCompleted.Add(type.Key, 0);
                TotalPickListsCompleted.Add(type.Key, 0);
                MaxPickListSize.Add(type.Key, 0);
                MinPickListSize.Add(type.Key, int.MaxValue);
                TotalPickingTime.Add(type.Key, TimeSpan.Zero);
            }

            NumItemsSorted = 0;
            MaxNumItemsSorted = 0;
            NumActiveSorters = 0;
            MaxActiveSorters = 0;

        }

        public double GetAverageNumActivePickers()
        {
            var duration = NumPickersJumpTime - StartTime;

            return AreaPickerTime.Ticks / duration.Ticks;
        }

        private TimeSpan MultiplyTimeSpan(TimeSpan duration, int multiplier)
        {
            duration = TimeSpan.FromTicks(duration.Ticks * multiplier);
            return duration;
        }

        private void AccrueAreaPickerTime()
        {
            AreaPickerTime += MultiplyTimeSpan(_sim.ClockTime - NumPickersJumpTime, NumActivePickers);
            NumPickersJumpTime = _sim.ClockTime;
        }

        public void IncrementActivePicker()
        {
            AccrueAreaPickerTime();

            NumActivePickers++;
            if (NumActivePickers > MaxActivePickers) MaxActivePickers = NumActivePickers;
        }

        public void DecrementActivePicker()
        {
            AccrueAreaPickerTime();

            NumActivePickers--;
        }

        public void IncrementActiveSorter()
        {
            NumActiveSorters++;
            if (NumActiveSorters > MaxActiveSorters) MaxActiveSorters = NumActiveSorters;
        }

        public void DecrementActiveSorter()
        {
            NumActiveSorters--;
        }

        public void CaptureCompletedPickList(Picker picker)
        {
            var numItems = picker.GetNumCompletedPickJobs();
            _sim.Scenario.CompletedPickLists[picker.Type].Add(picker.Picklist);
            TotalPickingTime[picker.Type] += picker.GetTimeToCompletePickList();
            TotalPickJobsCompleted[picker.Type] += numItems;
            TotalPickListsCompleted[picker.Type]++;

            if (numItems > MaxPickListSize[picker.Type])
                MaxPickListSize[picker.Type] = numItems;
            if (numItems < MinPickListSize[picker.Type])
                MinPickListSize[picker.Type] = numItems;

            // Send to consolidation
            _sim.Scenario.Consolidator.ProcessCompletedPicklist(_sim, picker.Picklist);
        }

        public TimeSpan GetAveragePickListTime(PickerType type)
        {
            if (TotalPickListsCompleted[type] == 0) return TimeSpan.Zero;

            return TimeSpan.FromSeconds(TotalPickingTime[type].TotalSeconds / TotalPickListsCompleted[type]);
        }

        private void CalculateOrderBatchesTotes()
        {
            OrderBatchesTotesCount = _sim.Scenario.OrderBatches.Select(b => b.PickLists.Count).ToList();
        }

        public int GetMaxOrderBatchesTotesCount()
        {
            if (OrderBatchesTotesCount == null) CalculateOrderBatchesTotes();

            return OrderBatchesTotesCount.Max();
        }

        public double GetAverageOrderBatchesTotesCount()
        {
            if (OrderBatchesTotesCount == null) CalculateOrderBatchesTotes();

            return OrderBatchesTotesCount.Average();
        }

        public double GetNumSortingStations()
        {
            return _sim.Scenario.Consolidator.AllSortingStations.Count;
        }

        public double GetAverageNumItemsSorted()
        {
            return 1.0 * NumItemsSorted / OrderBatch.GetTotalNumBatches();
        }
    }
}
