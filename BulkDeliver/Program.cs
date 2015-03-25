using BulkDeliver.Model;
using BulkDeliver.Optimizer;
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
                        Value_Mean = 120,
                        Value_Offset = 30,
                    }
                },
                DeliveryCost = CostProfile.GetCostProfile(5000, new double[] { 0, 10 }, new double[] { 1000, 9 }, new double[] { 2000, 8 }),
            };

            double cl = 0.99;
            Console.Write("Evaluating at {0:0}% confidence interval", cl * 100);
            var selection = new Selection(baseScenario, 
                (scenario, seed) => {
                    var sim = new Simulation(scenario, seed);
                    Console.Write(".");
                    sim.Run(TimeSpan.FromDays(365));
                    return sim.AverageAnnualCost;
                }, 
                new Decision(20, double.PositiveInfinity),
                new Decision(25, double.PositiveInfinity),
                new Decision(30, double.PositiveInfinity),
                new Decision(35, double.PositiveInfinity),
                new Decision(40, double.PositiveInfinity)
                );            
            selection.Evaluate(cl, 1000);
            Console.WriteLine();
            selection.Display();
        }

    }
}
