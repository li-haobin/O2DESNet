
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
            for (int seed = 0; seed < 20; seed++)
            {
                var simulation = new Simulation(TimeSpan.FromHours(1.0 / arrivalRate), TimeSpan.FromHours(1.0 / serviceRate), seed);
                simulation.Run(10000);
                Console.WriteLine("{0}\t{1}", seed, simulation.CustomerEventRecorder.AverageCount("Arrival", "Departure"));
            }
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Theoretical:\t{0}", arrivalRate / (serviceRate - arrivalRate));
        }
    }
}
