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
                InterArrivalTime = rs => TimeSpan.FromMinutes(Exponential.Sample(rs, 10)),

                //Server 1
                ServerCapacity1 = 3,
                ServiceTime1 = (l, rs) => TimeSpan.FromMinutes(Exponential.Sample(rs, 12)),
                MinInterDepartureTime1 = (l1, l2, rs) => TimeSpan.FromSeconds(5),

                BufferSize = 2,

                //Server 2
                ServerCapacity2 = 3,
                ServiceTime2 = (l, rs) => TimeSpan.FromMinutes(Exponential.Sample(rs, 12)),
                MinInterDepartureTime2 = (l1, l2, rs) => TimeSpan.FromSeconds(5),

                ToDepart = load => true,
            };

            var sim = new Simulator(new TwoFIFOServers(scenario));

            while (true)
            {
                sim.Run(1);// (speed: 1);
                Console.Clear();
                Console.WriteLine(sim.ClockTime);
                sim.Status.WriteToConsole();
                Console.ReadKey();
                //System.Threading.Thread.Sleep(1000);
            }
        }
    }
    
}
