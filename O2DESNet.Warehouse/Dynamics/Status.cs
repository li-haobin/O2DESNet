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
        public Dictionary<PickerType, int> MaxPickListSize { get; private set; }
        public Dictionary<PickerType, int> MinPickListSize { get; private set; }
        public Dictionary<PickerType, TimeSpan> TotalPickingTime { get; private set; }
        

        public Dictionary<PickerType, double> MinCartUtilisation { get; private set; }
        public Dictionary<PickerType, double> MaxCartUtilisation { get; private set; }
        public Dictionary<PickerType, List<double>> AllCartUtilisation { get; private set; }

        public TimeSpan TotalPickListWaitingTime { get; set; }
        
        public int NumActivePickers { get; private set; }
        public int MaxActivePickers { get; private set; }
        public TimeSpan AreaPickerTime { get; private set; }
        public DateTime NumPickersJumpTime { get; private set; }

        public double GetAverageNumActivePickers()
        {
            var duration = NumPickersJumpTime - StartTime;

            return AreaPickerTime.Ticks / duration.Ticks;
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

            var utilisation = picker.Picklist.GetUtilisation();
            AllCartUtilisation[picker.Type].Add(utilisation);
            if (utilisation > MaxCartUtilisation[picker.Type])
                MaxCartUtilisation[picker.Type] = utilisation;
            if (utilisation < MinCartUtilisation[picker.Type])
                MinCartUtilisation[picker.Type] = utilisation;

            // Send to consolidation
            _sim.Scenario.Consolidator.ProcessCompletedPicklist(_sim, picker.Picklist);
        }

        public TimeSpan GetAveragePickListTime(PickerType type)
        {
            if (TotalPickListsCompleted[type] == 0) return TimeSpan.Zero;

            return TimeSpan.FromSeconds(TotalPickingTime[type].TotalSeconds / TotalPickListsCompleted[type]);
        }
        #endregion

        #region Consolidator
        public List<int> OrderBatchesTotesCount { get; set; }
        public int NumItemsSorted { get; set; }
        public int MaxNumItemsSorted { get; set; }

        public Dictionary<OrderBatch, TimeSpan> OrderBatchCompletionTime { get; set; }

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
        public int GetMinOrderBatchesTotesCount()
        {
            if (OrderBatchesTotesCount == null) CalculateOrderBatchesTotes();

            return OrderBatchesTotesCount.Min();
        }
        public double GetAverageNumItemsSorted()
        {
            return 1.0 * NumItemsSorted / OrderBatch.GetTotalNumBatches();
        }

        

        // Sorters
        public int NumActiveSorters { get; private set; }
        public int MaxActiveSorters { get; private set; }
        public TimeSpan AreaSorterTime { get; private set; }
        public DateTime NumSortersJumpTime { get; private set; }

        private void AccrueAreaSorterTime()
        {
            AreaSorterTime += MultiplyTimeSpan(_sim.ClockTime - NumSortersJumpTime, NumActiveSorters);
            NumSortersJumpTime = _sim.ClockTime;
        }
        public void IncrementActiveSorter()
        {
            AccrueAreaSorterTime();
            NumActiveSorters++;
            if (NumActiveSorters > MaxActiveSorters) MaxActiveSorters = NumActiveSorters;
        }
        public void DecrementActiveSorter()
        {
            AccrueAreaSorterTime();
            NumActiveSorters--;
        }

        public double GetAverageNumActiveSorters()
        {
            var duration = NumSortersJumpTime - StartTime;
            return AreaSorterTime.Ticks / duration.Ticks;
        }

        // Number of batches in front of sorting station
        public Dictionary<OrderBatch, DateTime> OrderBatchStartWaitForSorting { get; set; }
        public Dictionary<OrderBatch, TimeSpan> OrderBatchWaitingTimeForSorting { get; set; }
        public int NumBatchesWaitingForSorting { get; private set; }
        public TimeSpan AreaBatchWaitingTime { get; private set; }
        public DateTime NumBatchWaitingJumpTime { get; private set; }

        private void AccrueAreaBatchWaitingTime()
        {
            AreaBatchWaitingTime += MultiplyTimeSpan(_sim.ClockTime - NumBatchWaitingJumpTime, NumBatchesWaitingForSorting);
            NumBatchWaitingJumpTime = _sim.ClockTime;
        }
        public void IncrementBatchWaiting()
        {
            AccrueAreaBatchWaitingTime();
            NumBatchesWaitingForSorting++;
        }
        public void DecrementBatchWaiting()
        {
            AccrueAreaBatchWaitingTime();
            NumBatchesWaitingForSorting--;
        }

        public double GetAverageNumBatchWaiting()
        {
            var duration = NumBatchWaitingJumpTime - StartTime;
            return AreaBatchWaitingTime.Ticks / duration.Ticks;
        }

        public int GetMaxNumSortingStations()
        {
            return _sim.Scenario.Consolidator.AllSortingStations.Count;
        }

        // Number of totes in front of sorting station
        public int NumTotesWaitingForSorting { get; private set; }
        public int MaxTotesWaitingForSorting { get; private set; }
        public TimeSpan AreaToteWaitingTime { get; private set; }
        public DateTime NumToteWaitingJumpTime { get; private set; }

        private void AccrueAreaToteWaitingTime()
        {
            AreaToteWaitingTime += MultiplyTimeSpan(_sim.ClockTime - NumToteWaitingJumpTime, NumTotesWaitingForSorting);
            NumToteWaitingJumpTime = _sim.ClockTime;
        }
        public void IncrementToteWaiting(int num)
        {
            AccrueAreaToteWaitingTime();
            NumTotesWaitingForSorting += num;
            if (NumTotesWaitingForSorting > MaxTotesWaitingForSorting) MaxTotesWaitingForSorting = NumTotesWaitingForSorting;
        }
        public void DecrementToteWaiting(int num)
        {
            AccrueAreaToteWaitingTime();
            NumTotesWaitingForSorting -= num;
        }

        public double GetAverageNumToteWaiting()
        {
            var duration = NumToteWaitingJumpTime - StartTime;
            return AreaToteWaitingTime.Ticks / duration.Ticks;
        }


        #endregion

        internal Status(Simulator simulation)
        {
            _sim = simulation;

            TotalPickJobsCompleted = new Dictionary<PickerType, int>();
            TotalPickListsCompleted = new Dictionary<PickerType, int>();
            TotalPickingTime = new Dictionary<PickerType, TimeSpan>();
            MaxPickListSize = new Dictionary<PickerType, int>();
            MinPickListSize = new Dictionary<PickerType, int>();
            MaxCartUtilisation = new Dictionary<PickerType, double>();
            MinCartUtilisation = new Dictionary<PickerType, double>();
            AllCartUtilisation = new Dictionary<PickerType, List<double>>();

            TotalPickListWaitingTime = TimeSpan.Zero;

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

                MaxCartUtilisation.Add(type.Key, 0.0);
                MinCartUtilisation.Add(type.Key, double.PositiveInfinity);
                AllCartUtilisation.Add(type.Key, new List<double>());
            }

            OrderBatchCompletionTime = new Dictionary<OrderBatch, TimeSpan>();
            NumItemsSorted = 0;
            MaxNumItemsSorted = 0;
            NumActiveSorters = 0;
            MaxActiveSorters = 0;
            AreaSorterTime = TimeSpan.Zero;
            NumSortersJumpTime = _sim.ClockTime;
            
            OrderBatchStartWaitForSorting = new Dictionary<OrderBatch, DateTime>();
            OrderBatchWaitingTimeForSorting = new Dictionary<OrderBatch, TimeSpan>();
            NumBatchesWaitingForSorting = 0;
            AreaBatchWaitingTime = TimeSpan.Zero;
            NumBatchWaitingJumpTime = _sim.ClockTime;

            NumTotesWaitingForSorting = 0;
            MaxTotesWaitingForSorting = 0;
            AreaToteWaitingTime = TimeSpan.Zero;
            NumToteWaitingJumpTime = _sim.ClockTime;
        }

        private TimeSpan MultiplyTimeSpan(TimeSpan duration, int multiplier)
        {
            duration = TimeSpan.FromTicks(duration.Ticks * multiplier);
            return duration;
        }
    }
}
