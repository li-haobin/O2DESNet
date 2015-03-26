using BulkDeliver.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver.Optimizer
{
    public class Decision
    {
        public TimeSpan TimeThreshold { get; private set; }
        public double WeightThreshold { get; private set; }
        public Decision(int daysThreshold, double weightThreshold)
        {
            TimeThreshold = TimeSpan.FromDays(daysThreshold);
            WeightThreshold = weightThreshold;
        }
        public Scenario GetScenario(Scenario baseScenario)
        {
            return new Scenario
            {
                TimeThreshold = TimeThreshold,
                WeightThreshold = WeightThreshold,
                ItemTypes = baseScenario.ItemTypes,
                DeliveryCost = baseScenario.DeliveryCost
            };
        }
        public override string ToString()
        {
            return string.Format("Threshold: {0} Days & {1} KG", TimeThreshold.TotalDays, WeightThreshold);
        }
    }
}
