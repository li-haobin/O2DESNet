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
            var sim = new Simulator(new Status(new Scenario(hourlyArrivalRate: 4, hourlyServiceRate: 5, serverCapacity: 1)));
            while (sim.Run(100000))
            {
                Console.WriteLine("{0}\t{1}",
                    sim.Status.Queue.HourCounter.AverageCount,
                    sim.Status.Processed.Average(l => l.TotalTimeSpan.TotalHours)
                    );
                Console.ReadKey();
            }
        }
    }
}
