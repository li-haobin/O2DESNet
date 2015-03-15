
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSimulator.Demos.MM1Queue
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.Write("Arrival Rate (per hour): ");
                double arrivalRate = Convert.ToDouble(Console.ReadLine()); //10 
                Console.Write("Service Rate (per hour): ");
                double serviceRate = Convert.ToDouble(Console.ReadLine()); //12
                Console.Write("Number of Events for each replication: ");
                int nEvents = Convert.ToInt32(Console.ReadLine());
                Console.Write("Number of replications: ");
                int nReplications = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("---------------------------------");
                Console.WriteLine("Seed\tAverageCount\tExecutionTime\tAnalysisTime");
                Console.WriteLine("---------------------------------");
                var timer = new Timer();
                for (int seed = 0; seed < nReplications; seed++)
                {
                    var simulation = new Simulation(TimeSpan.FromHours(1.0 / arrivalRate), TimeSpan.FromHours(1.0 / serviceRate), seed);
                    timer.Check();
                    simulation.Run(nEvents);
                    var executionTime = timer.Check();
                    var averageCount = simulation.CustomerEventRecorder.AverageCount("Arrival", "Departure");

                    Console.WriteLine("{0}\t{1:0.0000000}\t{2:0.0000000}\t{3:0.0000000}", 
                        seed, averageCount, executionTime.TotalSeconds, timer.Check().TotalSeconds);
                }
                Console.WriteLine("---------------------------------");
                Console.WriteLine("Theoretical:\t{0}", arrivalRate / (serviceRate - arrivalRate));
                Console.WriteLine("---------------------------------");
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
