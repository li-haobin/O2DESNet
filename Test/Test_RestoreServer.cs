using MathNet.Numerics.Distributions;
using O2DESNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_RestoreServer
{
    class Test_RestoreServer
    {
        static void Main(string[] args)
        {
            var hourlyArrivalRate = 4;
            var hourlyServiceRate = 5;
            var scenario = new Scenario
            {
                InterArrivalTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, hourlyArrivalRate)),
                ServiceTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, hourlyServiceRate)),
            };

            var sim = new Simulator(new Status(scenario));

            while (true)
            {
                sim.Run(speed: 10000);
                Console.Clear();
                Console.WriteLine(sim.ClockTime);
                sim.Status.WriteToConsole();
                System.Threading.Thread.Sleep(100);
            }            
        }
    }

    public class Scenario : O2DESNet.Scenario
    {
        public Func<Random, TimeSpan> InterArrivalTime { get; set; }
        public Func<Random, TimeSpan> ServiceTime { get; set; }
    }

    public class Status : Status<Scenario>
    {
        public class Load : Load<Scenario, Status> { }

        public Generator<Scenario, Status, Load> Generator { get; private set; }
        public Queue<Scenario, Status, Load> Queue1 { get; private set; }
        public RestoreServer<Scenario, Status, Load> Server1 { get; private set; }
        public Queue<Scenario, Status, Load> Queue2 { get; private set; }
        public RestoreServer<Scenario, Status, Load> Server2 { get; private set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            Generator = new Generator<Scenario, Status, Load>(
                statics: new Generator<Scenario, Status, Load>.StaticProperties
                {
                    InterArrivalTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, 3)), // arrival rate
                    Create = () => new Load(),
                    SkipFirst = false,
                },
                seed: DefaultRS.Next());
            Queue1 = new Queue<Scenario, Status, Load>(
                 statics: new Queue<Scenario, Status, Load>.StaticProperties
                 {
                     ToDequeue = () => Server1.Vancancy > 0,
                 },
                tag: "Queue1");
            Server1 = new RestoreServer<Scenario, Status, Load>(
                statics: new RestoreServer<Scenario, Status, Load>.StaticProperties
                {
                    Capacity = 1,
                    HandlingTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 7)), // handling rate
                    RestoringTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 10)), // restoring rate
                    ToDepart = () => Queue2.Vancancy > 0,
                },
                seed: DefaultRS.Next(),
                tag: "Server1");
            Queue2 = new Queue<Scenario, Status, Load>(
                statics: new Queue<Scenario, Status, Load>.StaticProperties
                {
                    Capacity = 4,
                    ToDequeue = () => Server2.Vancancy > 0,
                },
                tag: "Queue2");
            Server2 = new RestoreServer<Scenario, Status, Load>(
                statics: new RestoreServer<Scenario, Status, Load>.StaticProperties
                {
                    Capacity = 1,
                    HandlingTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 7)), // handling rate
                    RestoringTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 10)), // restoring rate
                    ToDepart = () => true,
                },
                seed: DefaultRS.Next(),
                tag: "Server2");

            Generator.OnArrive.Add(Queue1.Enqueue);
            Queue1.OnDequeue.Add(Server1.Start);
            Server1.OnDepart.Add(Queue2.Enqueue);
            Server1.OnRestore.Add(Queue1.Dequeue);
            Queue2.OnDequeue.Add(Server2.Start);
            Queue2.OnDequeue.Add(l => Server1.Depart());
            Server2.OnRestore.Add(Queue2.Dequeue);
        }

        public override void WarmedUp(DateTime clockTime)
        {
            Generator.WarmedUp(clockTime);
            Queue1.WarmedUp(clockTime);
            Queue2.WarmedUp(clockTime);
            Server1.WarmedUp(clockTime);
            Server2.WarmedUp(clockTime);
        }

        public override void WriteToConsole()
        {
            Queue1.WriteToConsole(); Console.WriteLine();
            Server1.WriteToConsole(); Console.WriteLine();
            Queue2.WriteToConsole(); Console.WriteLine();
            Server2.WriteToConsole(); Console.WriteLine();
            Console.WriteLine("Completed: {0}", Server2.NCompleted);
        }
    }

    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            Execute(Status.Generator.Start());
        }
    }
}
