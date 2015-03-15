
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
            double arrivalRate = 10, serviceRate = 12;
            Console.WriteLine("Seed\tAverage Count");
            var timer = new Timer();
            for (int seed = 0; seed < 20; seed++)
            {
                var simulation = new Simulation(TimeSpan.FromHours(1.0 / arrivalRate), TimeSpan.FromHours(1.0 / serviceRate), seed);
                timer.Check();
                simulation.Run(10000);
                var executionTime = timer.Check();
                var averageCount = simulation.CustomerEventRecorder.AverageCount("Arrival", "Departure");

                Console.WriteLine("{0}\t{1}\t{2}\t{3}", seed, averageCount, executionTime.TotalMilliseconds, timer.Check().TotalMilliseconds);
            }
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Theoretical:\t{0}", arrivalRate / (serviceRate - arrivalRate));
        }
    }
}
