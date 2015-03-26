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
        public virtual ICollection<ItemType> ItemTypes { get; set; }
        public virtual CostProfile DeliveryCost { get; set; }
        public override string ToString()
        {
            var str = "================================================\nItems:\nRate\tWeight\t\tValue\n";
            foreach (var i in ItemTypes) 
                str += string.Format("{0}\t{1}/{2}kg\t{3}/{4}kg\n", 30.0 / i.IAT_Expected.TotalDays, i.Weight_Mean, i.Weight_Offset, i.Value_Mean, i.Value_Offset);
            str += "================================================\nDelivery Cost:\n";
            str += string.Format("${0}", DeliveryCost.Constan);
            foreach (var i in DeliveryCost.Pieces)
                str += string.Format(" + ${0}/kg (abv.{1}kg)", i.UnitCost, i.StartWeight);
            str += "\n================================================";
            return str;
        }
    }
}
