using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace O2DESNet.Demos.GGnQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            var hourlyArrivalRate = 4;
            var hourlyServiceRate = 5;

            var scenario = 
                new GGnQueueSystem.Statics
                {
                    InterArrivalTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, hourlyArrivalRate)), // G: Inter-Arrival-Time Distribution
                    ServiceTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, hourlyServiceRate)), // G: Service Time Distribution
                    ServerCapacity = 1, // n: number of concurrent servers
                };

            var sim = new Simulator(assembly: new GGnQueueSystem(scenario, seed: 0));

            while (true)
            {
                sim.Run(speed: 1000);
                Console.Clear();
                Console.WriteLine(sim.ClockTime);
                sim.Status.WriteToConsole();
                System.Threading.Thread.Sleep(100);
            }

            /// Validate by Little's Law
            /// Theoretical Output: 3.2 & 1.0
            //while (sim.Run(300000))
            //{
            //    Console.WriteLine("{0}\t{1}",
            //        ((GGnQueueSystem)sim.Status).Queue.HourCounter.AverageCount,
            //        ((GGnQueueSystem)sim.Status).Processed.Average(l => l.TotalTimeSpan.TotalHours)
            //        );
            //    Console.ReadKey();
            //}
        }
    }
    
}
