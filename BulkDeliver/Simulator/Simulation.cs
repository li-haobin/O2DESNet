using BulkDeliver.Model;
using CSharpSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver.Simulator
{
    public class Simulation : DESModel
    {
        internal Scenario Scenario;
        private Dictionary<ItemType, Random> _randomStreams;
        public Status_Bulk Status_Bulk { get; private set; }
        public double TotalInventoryCost { get; private set; }
        public double TotalDeliveryCost { get; private set; }
        public double AverageAnnualCost { get { return (TotalDeliveryCost + TotalInventoryCost) / (ClockTime - DateTime.MinValue).TotalDays * 365; } }
        public int TotalCycleCount { get; private set; }

        public Simulation(Scenario scenario, int seed)
        {
            Scenario = scenario;            
            _randomStreams = new Dictionary<ItemType,Random>();
            var rs = new Random(seed);
            foreach (var itemType in Scenario.ItemTypes)
            {
                _randomStreams.Add(itemType, new Random(rs.Next()));
                ScheduleArrival(itemType);
            }
            Status_Bulk = new Status_Bulk(this);
            TotalDeliveryCost = 0;
            TotalInventoryCost = 0;
            TotalCycleCount = 0;
        }
        public override void Run(TimeSpan duration)
        {
            var target = ClockTime + duration;
            while (ClockTime < target) Run(1);
        }
        public override void Run(int cycleCount){
            var target = TotalCycleCount + cycleCount;
            while (TotalCycleCount < target) if (!ExecuteHeadEvent()) break;
        }
        private void ScheduleArrival(ItemType itemType)
        {
            var rs = _randomStreams[itemType];
            ScheduleEvent(Arrive(new Load_Item(itemType, rs)), RandomTime.Exponential(itemType.IAT_Expected, rs));
        }

        private Event Arrive(Load_Item item)
        {
            return delegate()
            {
                item.ArrivalTime = ClockTime;
                ScheduleArrival(item.Type);
                Status_Bulk.Push(item);
                if (Status_Bulk.ToDeliver) Deliver()(); // deliver now if any threshold is reached
                else if (Status_Bulk.Count < 2) ScheduleEvent(Check(), Scenario.TimeThreshold); // check later if the item is the first
            };
        }
        private Event Deliver()
        {
            return delegate()
            {
                double deliveryCost, sumInventoryCost;
                Status_Bulk.Deliver(out deliveryCost, out sumInventoryCost);
                TotalDeliveryCost += deliveryCost;
                TotalInventoryCost += sumInventoryCost;
                TotalCycleCount++;
            };
        }
        private Event Check()
        {
            return delegate()
            {
                if (Status_Bulk.ToDeliver) Deliver()();
            };
        }
    }
}
