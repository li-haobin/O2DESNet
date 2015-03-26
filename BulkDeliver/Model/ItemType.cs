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
    }
}
