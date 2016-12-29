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
            var scenario = new TwoRestoreServerSystem.Statics
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
            };

            var sim = new Simulator(new TwoRestoreServerSystem(scenario));

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
    
}
