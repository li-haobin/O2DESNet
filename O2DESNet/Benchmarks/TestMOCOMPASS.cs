using O2DESNet.Explorers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Benchmarks
{
    class TestMOCOMPASS
    {
        static void Main(string[] args)
        {
            var dim = 10;
            var mocompass = new MoCompass<Benchmark, Status, Simulator>(
                ZDTx.DecisionSpace(dim),
                decisions => new ZDT1(decisions, new double[] { 0, 0 }),
                (scenario, seed) => new Status(scenario, seed),
                status => new Simulator(status),
                status => true,
                status => status.Objectives,
                inParallel: false
                );

            while (true)
            {
                mocompass.Iterate(10, 50);
                Console.Clear();
                foreach (var s in mocompass.ParetoSet.OrderBy(s => mocompass.Replicator.GetObjMeans(s)[0]))
                {
                    foreach (var o in s.CalObjectives()) Console.Write("{0:F4},", o);
                    Console.WriteLine();
                }
                Console.ReadKey();
            }
        }
    }
}
