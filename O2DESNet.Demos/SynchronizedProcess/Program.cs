using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace O2DESNet.Demos.SynchronizedProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            var scenario = new SynchronizedProcess.Statics
            {
                GeneratorA = new Generator<Load>.Statics {
                    Create = rs=> new Load(),
                    InterArrivalTime = rs=> TimeSpan.FromHours(Exponential.Sample(rs, 10)),
                },
                GeneratorB = new Generator<Load>.Statics
                {
                    Create = rs => new Load(),
                    InterArrivalTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, 8)),
                },
                QueueA = new Queuing<Load>.Statics(),
                QueueB = new Queuing<Load>.Statics(),
                ServerA1 = new Server<Load>.Statics
                {
                    Capacity = 2,
                    ServiceTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 12)),
                },
                ServerA2 = new Server<Load>.Statics
                {
                    Capacity = 2,
                    ServiceTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 15)),
                },
                ServerB1 = new Server<Load>.Statics
                {
                    Capacity = 2,
                    ServiceTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 10)),
                },
                ServerB2 = new Server<Load>.Statics
                {
                    Capacity = 2,
                    ServiceTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 10)),
                },
            };

            var sim = new Simulator(new SynchronizedProcess(scenario, 0));

            //while (true)
            //{
            //    sim.Run(speed: 100);
            //    Console.Clear();
            //    Console.WriteLine(sim.ClockTime);
            //    sim.Status.WriteToConsole();
            //    System.Threading.Thread.Sleep(200);
            //}

            while (true)
            {
                sim.Run(1);
                Console.WriteLine("=====================================");
                Console.WriteLine(sim.ClockTime);
                sim.Status.WriteToConsole();
                Console.ReadKey();
            }
        }
    }
    
}
