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
                ServerCapacity1 = 2,
                ServiceTime1 = (l, rs) => TimeSpan.FromMinutes(Exponential.Sample(rs, 12)),
                BufferSize = 2,
                //Server 2
                ServerCapacity2 = 2,
                ServiceTime2 = (l, rs) => TimeSpan.FromMinutes(Exponential.Sample(rs, 12)),
            };
            //var sim = new Simulator(new TwoFIFOServers(scenario));
            var sim = new Simulator(new TwoFIFOServers(scenario));
            while (true)
            {
                sim.Run(1);// (speed: 1);
                //Console.Clear();
                Console.WriteLine("\n=========================\n");
                Console.WriteLine(sim.ClockTime);
                sim.State.WriteToConsole();
                Console.ReadKey();
                //System.Threading.Thread.Sleep(1000);
            }
        }
    }
}