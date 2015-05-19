using BulkDeliver.Model;
using BulkDeliver.Optimizer;
using BulkDeliver.Simulator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver
{
    class Experiment
    {
        static Scenario[] TestScenarios
        {
            get
            {
                var itemTypes = new ItemType[] { 
                    ItemType.GetExample(false, false, false),
                    ItemType.GetExample(false, false, true),
                    ItemType.GetExample(false, true, false),
                    ItemType.GetExample(false, true, true),
                    ItemType.GetExample(true, false, false),
                    ItemType.GetExample(true, false, true),
                    ItemType.GetExample(true, true, false),
                    ItemType.GetExample(true, true, true),
                };
                return new Scenario[] { 
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[0] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[1] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[2] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[3] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[4] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[5] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[6] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[7] }},

                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[0], itemTypes[1] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[2], itemTypes[3] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[4], itemTypes[5] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[6], itemTypes[7] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[0], itemTypes[2] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[1], itemTypes[3] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[4], itemTypes[6] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[5], itemTypes[7] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[0], itemTypes[4] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[1], itemTypes[5] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[2], itemTypes[6] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[3], itemTypes[7] }},

                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[0], itemTypes[1], itemTypes[2], itemTypes[3] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[4], itemTypes[5], itemTypes[6], itemTypes[7] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[0], itemTypes[1], itemTypes[4], itemTypes[5] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[2], itemTypes[3], itemTypes[6], itemTypes[7] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[0], itemTypes[2], itemTypes[4], itemTypes[6] }},
                    new Scenario{ ItemTypes = new ItemType[] { itemTypes[1], itemTypes[3], itemTypes[5], itemTypes[7] }},
                };
            }
        }

        static void Main()
        {
            var cl = 0.95;
            int budget = 1000;
            int runLengthDays = 365;

            int count = 0;
            foreach (var baseScenario in TestScenarios)
            {
                count++;
                foreach (var costProfile in new CostProfile[] { CostProfile.ExampleAirfreight, CostProfile.ExampleContainer, CostProfile.ExamplePallete })
                {
                    Console.WriteLine("{0}/{1} {2}", ++count, TestScenarios.Count(), costProfile.Name);
                    baseScenario.DeliveryCost = costProfile;
                    using (var sw = new StreamWriter(baseScenario.Name + ".csv"))
                    {
                        sw.WriteLine("Day,kg,$");
                        for (int day = 5; day <= 90; day += 5)
                        {
                            //var day = 1000; // not effecitve
                            var decisions = Enumerable.Range(1, 50).Select(v => new Decision(day, v * 100)).ToArray();
                            var selection = new Selection(baseScenario,
                                (scenario, seed) =>
                                {
                                    var sim = new Simulation(scenario, seed);
                                    sim.Run(TimeSpan.FromDays(runLengthDays));
                                    return sim.AverageAnnualCost;
                                },
                                decisions
                                );
                            selection.Evaluate(cl, budget, true);

                            //selection.Display();
                            var decision = selection.Optima.OrderBy(d => d.WeightThreshold).First();
                            Console.WriteLine("Day {0} -> {1} kg (${2})", day, decision.WeightThreshold, selection.Statistics[decision].Mean);
                            sw.WriteLine("{0},{1},{2}", day, decision.WeightThreshold, selection.Statistics[decision].Mean);
                        }
                    }
                }
            }
        }
    }
}
