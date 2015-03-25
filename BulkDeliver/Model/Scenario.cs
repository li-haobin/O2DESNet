using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver.Model
{
    public class Scenario
    {
        public int Id { get; set; }
        /// <summary>
        /// The time threshold for a bulk delivery
        /// </summary>
        public TimeSpan TimeThreshold { get; set; }
        /// <summary>
        /// The weight threshold for a builk delivery
        /// </summary>
        public double WeightThreshold { get; set; }
        public double DailyInventoryCostRatio { get; set; }
        public virtual ICollection<ItemType> ItemTypes { get; set; }
        public virtual CostProfile DeliveryCost { get; set; }
    }
}
