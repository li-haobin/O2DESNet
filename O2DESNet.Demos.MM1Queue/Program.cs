using System;
using System.Linq;

namespace O2DESNet.Demos.MM1Queue
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
                Console.WriteLine("Seed\tAve.Count\tAve.Duration(h)\tSimulation Time(h)\tExecution Time(s)");
                Console.WriteLine("---------------------------------");

                var scenario = new Scenario(TimeSpan.FromHours(1.0 / arrivalRate), TimeSpan.FromHours(1.0 / serviceRate));
                for (int seed = 0; seed < nReplications; seed++)
                {
                    var sim = new Simulator(scenario, seed);
                    var timestamp = DateTime.Now;
                    sim.Run(nEvents);
                    Console.WriteLine("{0}\t{1:0.0000000}\t{2:0.0000000}\t{3:0.0000000}\t{4:0.0000000}",
                        seed, sim.Status.InSystemCounter.AverageCount, 
                        sim.Status.ServedCustomers.Average(c=>c.InSystemDuration.TotalHours), 
                        sim.Status.InSystemCounter.TotalHours, 
                        (DateTime.Now - timestamp).TotalSeconds);
                }
                Console.WriteLine("---------------------------------");
                var expectedCount = arrivalRate / (serviceRate - arrivalRate);
                var expectedDuration = expectedCount / arrivalRate;
                Console.WriteLine("Expected Count:\t{0:0.0000000}", expectedCount);
                Console.WriteLine("Expected Duration(h):\t{0:0.0000000}", expectedDuration);
                Console.WriteLine("---------------------------------");
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
