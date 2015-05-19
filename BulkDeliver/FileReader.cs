using BulkDeliver.Model;
using BulkDeliver.Optimizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver
{
    public static class FileReader
    {
        public static Scenario GetScenario()
        {
            return new Scenario
            {
                ItemTypes = GetItems(),
                DeliveryCost = GetCostProfile(),
            };
        }
        private static List<ItemType> GetItems()
        {
            var items = new List<ItemType>();
            using (var sr = new StreamReader("input_item_types.csv"))
            {
                string line;
                sr.ReadLine();
                while ((line = sr.ReadLine()) != null)
                {
                    var data = line.Split(',');
                    var monthlyRate = Convert.ToDouble(data[0]);
                    var minWeight = Convert.ToDouble(data[1]);
                    var maxWeight = Convert.ToDouble(data[2]);
                    var minValue = Convert.ToDouble(data[3]);
                    var maxValue = Convert.ToDouble(data[4]);
                    items.Add(new ItemType
                    {
                        IAT_Expected = TimeSpan.FromDays(30.0 / monthlyRate),
                        Weight_Mean = (minWeight + maxWeight) / 2,
                        Weight_Offset = (maxWeight - minWeight) / 2,
                        Value_Mean = (minValue + maxValue) / 2,
                        Value_Offset = (maxValue - minValue) / 2,
                        DailyInventoryCostRatio = Convert.ToDouble(data[5]),
                    });
                }
            }
            return items;
        }
        private static CostProfile GetCostProfile()
        {
            double constan;
            var pieces = new List<double[]>();
            using (var sr = new StreamReader("input_delivery_cost.csv"))
            {
                constan = Convert.ToDouble(sr.ReadLine().Split(',')[1]);
                sr.ReadLine(); sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                    pieces.Add(line.Split(',').Select(d => Convert.ToDouble(d)).ToArray());
            }
            return CostProfile.GetCostProfile("", constan, pieces);
        }
        public static Decision[] GetDecisions()
        {
            var decisions = new List<Decision>();
            using (var sr = new StreamReader("input_decisions.csv"))
            {
                string line;
                sr.ReadLine();
                while ((line = sr.ReadLine()) != null)
                {
                    var data = line.Split(',');
                    decisions.Add(new Decision(Convert.ToInt32(data[0]), Convert.ToDouble(data[1])));
                }
            }
            return decisions.ToArray();
        }
    }
}
