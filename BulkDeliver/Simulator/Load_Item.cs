using BulkDeliver.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver.Simulator
{
    public class Load_Item
    {
        public ItemType Type { get; private set; }
        public DateTime ArrivalTime { get; internal set; }
        public double Weight { get; private set; }
        public double Value { get; private set; }
        public Load_Item(ItemType type, Random rs)
        {
            Type = type;
            Weight = (type.Weight_Mean - type.Weight_Offset) + type.Weight_Offset * rs.NextDouble() * 2;
            Value = (type.Value_Mean - type.Value_Offset) + type.Value_Offset * rs.NextDouble() * 2;
        }
    }
}
