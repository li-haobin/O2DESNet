using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var sim = new Simulator(new Status(new Scenario(hourlyArrivalRate: 4, hourlyServiceRate: 5)));
            while (sim.Run(10000))
            {
                Console.WriteLine("{0}\t{1}",
                    sim.Status.Queue.HourCounter.AverageCount,
                    sim.Status.Processed.Average(l => l.TimeSpan_InSystem.TotalHours)
                    );
                Console.ReadKey();
            }
        }
    }
}
