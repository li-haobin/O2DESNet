using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    class TestOCBA_AgeOfInformation
    {
        static void Main(string[] args)
        {
            int nSeeds = 100;
            for (int aoi = 20; aoi <= 40; aoi += 20)
            {
                Console.WriteLine("Age of Information: {0}", aoi);
                var stats = new AverageRunningStats(nSeeds);
                Parallel.ForEach(Enumerable.Range(0, nSeeds), seed =>
                {
                    var rns = new Benchmarks.RnS_SlippageConfiguration(30, 0.1, seed);
                    var tasks = TaskList(new OCBA().Alloc(aoi, rns.Solutions));
                    while (true)
                    {          
                        rns.Evaluate(tasks.First(), 1);
                        tasks.RemoveAt(0);
                        var count = rns.Solutions.Sum(s => s.Observations.Count);
                        lock (stats) stats.Log(seed, count, rns.PCS);
                        if (count > 10000) break;
                        tasks.AddRange(TaskList(new OCBA().Alloc(aoi - tasks.Count, rns.Solutions)));
                    }
                    Console.Write("x");
                });
                using (var sw = new System.IO.StreamWriter(string.Format("aoi_{0}.csv", aoi)))
                    foreach (var l in stats.Output) sw.WriteLine("{0},{1}", l.Item1, l.Item2);
            }
        }

        private static List<int> TaskList(Dictionary<DenseVector,int> alloc)
        {
            var list = new List<int>();
            foreach (var a in alloc) list.AddRange(Enumerable.Repeat((int)a.Key.First(), a.Value));
            return list;
        }

    }
}
