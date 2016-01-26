using O2DESNet.Replicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Benchmarks
{
    class Test
    {
        static void Main(string[] args)
        {
            var rs = new Random(0);
            var benchmarks = new List<Benchmark>();
            for (int i = 0; i < 10; i++)
                benchmarks.Add(new ZDT1(Enumerable.Range(0, 10).Select(j => rs.NextDouble()).ToArray(), new double[] { 0.1, 0.1 }));

            var paretoFinder = new ParetoFinder<Benchmark, Status, Simulator>(
                scenarios: benchmarks,
                constrStatus: (scenario, seed) => new Status(scenario, seed),
                constrSimulator: status => new Simulator(status),
                terminate: status => true,
                objectives: status => status.Objectives);
            paretoFinder.Display();
        }
    }
}
