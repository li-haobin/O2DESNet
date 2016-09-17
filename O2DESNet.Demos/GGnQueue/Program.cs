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
            var scenario = new Scenario
            {
                InterArrivalTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, hourlyArrivalRate)), // G: Inter-Arrival-Time Distribution
                ServiceTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, hourlyServiceRate)), // G: Service Time Distribution
                ServerCapacity = 1, // n: number of concurrent servers
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

            // Validate by Little's Law
            //while (sim.Run(300000))
            //{
            //    Console.WriteLine("{0}\t{1}",
            //        sim.Status.GGnQueueSystem.Queue.HourCounter.AverageCount,
            //        sim.Status.GGnQueueSystem.Processed.Average(l => l.TotalTimeSpan.TotalHours)
            //        );
            //    Console.ReadKey();
            //}
        }
    }

    public class Load : Load<Scenario, Status> { }
    public class Scenario : GGnQueueSystem<Scenario, Status, Load>.StaticProperties
    {
        public Scenario() { Create = () => new Load(); }
    }
    public class Status : Status<Scenario>
    {
        public GGnQueueSystem<Scenario, Status, Load> GGnQueueSystem { get; private set; }        
        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            GGnQueueSystem = new GGnQueueSystem<Scenario, Status, Load>(scenario, seed);            
        }
        public override void WarmedUp(DateTime clockTime) { GGnQueueSystem.WarmedUp(clockTime); }
        public override void WriteToConsole() { GGnQueueSystem.WriteToConsole(); }
    }
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status) { Execute(Status.GGnQueueSystem.Start()); }
    }
}
