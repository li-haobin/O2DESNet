using BulkDeliver.Model;
using BulkDeliver.Simulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseScenario = new Scenario
            {
                DailyInventoryCostRatio = 0.005,
                ItemTypes = new ItemType[]{
                    new ItemType{
                        IAT_Expected = TimeSpan.FromDays(0.03), // 1000 items per 30 days
                        Weight_Mean = 2.5,
                        Weight_Offset = 0.5,
                        Value_Mean = 0.6,
                        Value_Offset = 0.2,
                    }
                },
                DeliveryCost = CostProfile.GetCostProfile(500, new double[] { 0, 10 }, new double[] { 1000, 9 }, new double[] { 2000, 8 }),
            };



            var simulation = new Simulation(baseScenario, 1);
            while (true)
            {
                simulation.Run(TimeSpan.FromDays(1000));
                Console.WriteLine(simulation.AverageAnnualCost);
                Console.ReadKey();
            }

        }
    }
}
