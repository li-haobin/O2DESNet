using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.GG1Queue
{
    class Program
    {
        static void Main(string[] args)
        {
            var scenario = new Scenario { HourlyArrivalRate = 4, HourlyServiceRate = 5 };
            var status = new Status(scenario, 0) {
                //Display = true
            };
            var sim = new Simulator(status);
            while (sim.Run(10000))
            {
                Console.WriteLine("{0}\t{1}",
                    status.Queue.HourCounter.AverageCount,
                    status.Processed.Average(l => l.TimeSpan_InSystem.TotalHours)
                    );
                Console.ReadKey();
            }
        }
    }
}
