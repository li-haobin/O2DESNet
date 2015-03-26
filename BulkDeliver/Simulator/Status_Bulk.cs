using BulkDeliver.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver.Simulator
{
    public class Status_Bulk
    {
        private Simulation _simulation;
        private List<Load_Item> _items;

        public Status_Bulk(Simulation simulation)
        {
            _simulation = simulation;
            _items = new List<Load_Item>();
        }
        public void Push(Load_Item item) { _items.Add(item); }
        public int Count { get { return _items.Count; } }
        public bool ToDeliver
        {
            get
            {
                return Count > 0 && (
                    (_simulation.Scenario.WeightThreshold > 0 && _items.Sum(i => i.Weight) >= _simulation.Scenario.WeightThreshold) ||
                    (_simulation.Scenario.TimeThreshold.TotalDays > 0 && _simulation.ClockTime - _items.First().ArrivalTime >= _simulation.Scenario.TimeThreshold)
                    );
            }
        }
        public void Deliver(out double deliveryCost, out double sumInventoryCost)
        {
            deliveryCost = _simulation.Scenario.DeliveryCost.Calculate(_items.Sum(i => i.Weight));
            sumInventoryCost = _items.Sum(i => i.Value * i.Type.DailyInventoryCostRatio * (_simulation.ClockTime - i.ArrivalTime).TotalDays);
            _items = new List<Load_Item>();
        }
    }
}
