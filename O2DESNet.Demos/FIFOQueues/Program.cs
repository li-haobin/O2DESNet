using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace O2DESNet.Demos.FIFOQueues
{
    class Program
    {
        static void Main(string[] args)
        {
            var scenario = new TwoFIFOServers.Statics
            {
                InterArrivalTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, 10)),

                //Server 1
                ServerCapacity1 = 3,
                ServiceTime1 = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 4)),

                BufferSize = 2,

                //Server 2
                ServerCapacity2 = 3,
                ServiceTime2 = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 4)),

                ToDepart = load => true,
            };

            var sim = new Simulator(new TwoFIFOServers(scenario));

            while (true)
            {
                sim.Run(speed: 100);
                Console.Clear();
                Console.WriteLine(sim.ClockTime);
                sim.Status.WriteToConsole();
                System.Threading.Thread.Sleep(100);
            }
        }
    }
    
}
