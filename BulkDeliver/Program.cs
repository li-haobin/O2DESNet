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
            while (true)
            {
                var baseScenario = FileReader.GetScenario();
                var decisions = FileReader.GetDecisions();

                Console.Clear();
                Console.WriteLine("====================================");
                Console.WriteLine("Scenario:");
                Console.WriteLine(baseScenario);
                Console.WriteLine("\n====================================");
                Console.WriteLine("Decisions:");
                foreach (var d in decisions) Console.WriteLine(d);
                Console.WriteLine("====================================");

                Console.WriteLine();
                Console.Write("# of Days to simulate for each replication: ");
                double days = Convert.ToDouble(Console.ReadLine());
                Console.Write("Confidence Level Required (0.01 ~ 1.00): ");
                double cl = Convert.ToDouble(Console.ReadLine());
                Console.Write("Max # of total relications: ");
                int budget = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("\nEvaluating at {0:0}% confidence interval...", cl * 100);
                var selection = new Selection(baseScenario,
                    (scenario, seed) =>
                    {
                        var sim = new Simulation(scenario, seed);
                        sim.Run(TimeSpan.FromDays(days));
                        return sim.AverageAnnualCost;
                    },
                    decisions
                    );
                selection.Evaluate(cl, budget, true);
                Console.WriteLine("\n");
                selection.Display();

                Console.Write("\nEvaluate again (Y/N)? ");
                if (Console.ReadLine().ToUpper() != "Y") break;
            }
        }


    }
}
