using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Events
{
    [Serializable]
    internal class CompleteSorting : Event
    {
        internal SortingStation sortingStation;

        internal CompleteSorting(Simulator sim, SortingStation station) : base(sim)
        {
            sortingStation = station;
        }


        public override void Invoke()
        {
            _sim.Scenario.Consolidator.NumSortersAvailable++;
            _sim.Status.DecrementActiveSorter();

            // Check ReadyToQueue
            if (_sim.Scenario.Consolidator.ReadyToSort.Count > 0)
            {
                var toSortNext = _sim.Scenario.Consolidator.ReadyToSort.Dequeue();
                _sim.ScheduleEvent(new BeginSorting(_sim, toSortNext), _sim.ClockTime); // sort next one now
            }

        }

        public override void Backtrack()
        {
            throw new NotImplementedException();
        }
    }
}
