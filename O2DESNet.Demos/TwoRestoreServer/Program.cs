using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace O2DESNet.Demos.TwoRestoreServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var scenario = new Scenario
            {
                InterArrivalTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, 3)),

                //Server 1
                ServerCapacity1 = 1,
                HandlingTime1 = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 7)),
                RestoringTime1 = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 10)),

                BufferSize = 4,

                //Server 2
                ServerCapacity2 = 1,
                HandlingTime2 = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 7)),
                RestoringTime2 = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 10)),

                ToDepart = load => true,
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
        }
    }

    public class Load : Load<Scenario, Status> { }
    public class Scenario : TwoRestoreServerSystem<Scenario, Status, Load>.StaticProperties
    {
        public Scenario() { Create = () => new Load(); }
    }
    public class Status : Status<Scenario>
    {
        public TwoRestoreServerSystem<Scenario, Status, Load> System { get; private set; }
        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            System = new TwoRestoreServerSystem<Scenario, Status, Load>(scenario, seed);
        }
        public override void WarmedUp(DateTime clockTime) { System.WarmedUp(clockTime); }
        public override void WriteToConsole() { System.WriteToConsole(); }
    }
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            Execute(Status.System.Start());
        }
    }
}
