using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var hourlyArrivalRate = 4;
            var hourlyServiceRate = 5;
            var scenario = new Scenario
            {
                InterArrivalTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, hourlyArrivalRate)),
                ServiceTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, hourlyServiceRate)),
                ServerCapacity = 1
            };

            var sim = new Simulator(new Status(scenario));

            while (true)
            {
                sim.Run(speed: 1000);
                Console.Clear();
                Console.WriteLine(sim.ClockTime);
                sim.Status.WriteToConsole();
                System.Threading.Thread.Sleep(100);
            }

            //while (sim.Run(300000))
            //{
            //    Console.WriteLine("{0}\t{1}",
            //        sim.Status.GGnQueue.Queue.HourCounter.AverageCount,
            //        sim.Status.Processed.Average(l => l.TotalTimeSpan.TotalHours)
            //        );
            //    Console.ReadKey();
            //}
        }
    }
}
