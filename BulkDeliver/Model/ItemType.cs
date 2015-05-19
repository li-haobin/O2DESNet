using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver.Model
{
    public class ItemType
    {
        public int Id { get; set; }
        public TimeSpan IAT_Expected { get; set; }
        public double Weight_Mean { get; set; } //in kg
        public double Weight_Offset { get; set; } //in kg
        public double Value_Mean { get; set; } // in dollar
        public double Value_Offset { get; set; } // in dollar
        public double DailyInventoryCostRatio { get; set; }
        public static ItemType GetExample(bool highDemand, bool highWeight, bool highValue){
            var itemType = new ItemType { DailyInventoryCostRatio = 0.001 };
            if (highDemand) itemType.IAT_Expected = TimeSpan.FromDays(30.0 / 3000);
            else itemType.IAT_Expected = TimeSpan.FromDays(30.0 / 200);
            double lb, ub;
            
            // weight
            if (highWeight) { lb = 0.5; ub = 3.0; }
            else { lb = 10.0; ub = 30.0; }
            itemType.Weight_Mean = (ub + lb) / 2;
            itemType.Weight_Offset = (ub - lb) / 2;
            
            // value
            if (highValue) { lb = 1000; ub = 3000; }
            else { lb = 50; ub = 200; }
            itemType.Value_Mean = (ub + lb) / 2;
            itemType.Value_Offset = (ub - lb) / 2;

            return itemType;
        }
    }
}
