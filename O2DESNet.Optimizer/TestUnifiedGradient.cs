using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using O2DESNet.Optimizer.Samplings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    class TestUnifiedGradient
    {
        static void Main(string[] args)
        {
            //int dimension = 3;
            //int paretoSize = 10;

            //var rs = new Random(0);
            //var paretoSet = new List<DenseVector>();
            //while (paretoSet.Count < paretoSize)
            //{
            //    paretoSet.Add(Enumerable.Range(0, dimension).Select(i => rs.NextDouble()).ToArray());
            //    paretoSet = Pareto.GetParetoSet(paretoSet).ToList();
            //}

            //var weights = Pareto.UnifiedWeightingVectors(paretoSet, Enumerable.Range(0, dimension).Select(i => 1.0).ToArray());
            //foreach (var p in paretoSet.OrderBy(p => p[0]).ThenBy(p => p[1]))
            //{
            //    Console.Write("{0},{1},{2},,", p[0], p[1], p[2]);
            //    var w = weights[p];
            //    Console.WriteLine("{0},{1},{2}", w[0], w[1], w[2]);
            //}

            Experiment(length: 1000, nSeeds: 30);
        }    
        
        static void Experiment(int length, int nSeeds)
        {
            int dimension = 10;
            int batchSize = 10;
            var zdt = new Benchmarks.ZDT1(dimension);

            var stats = new AverageRunningStats(nSeeds);
            for (int seed = 0; seed < nSeeds; seed++)
            {
                var rs = new Random(seed);
                var mocompass = new MoCompass(zdt.DecisionSpace, MoCompass.SamplingScheme.GoPolars, seed: rs.Next());
                
                while (mocompass.AllSolutions.Count < length)
                {
                    DenseVector[] decs;
                    if (mocompass.ParetoSet.Length > 0) decs = mocompass.Sample(batchSize, decimals: 2, nTrials: 1000);
                    else decs = Enumerable.Range(0, batchSize).Select(i => (DenseVector)Enumerable.Range(0, dimension).Select(j => rs.NextDouble()).ToArray()).ToArray(); // randomize initial solutions
                    mocompass.Enter(decs.Select(d =>
                    {
                        var sol = new StochasticSolution(d, zdt.Evaluate(d));
                        sol.Gradients = zdt.Gradients(d);
                        return sol;
                    }));
                    stats.Log(seed, mocompass.AllSolutions.Count, 
                        Pareto.DominatedHyperVolume(mocompass.ParetoSet.Select(s => s.Objectives), new double[] { 1, 1 }));
                    Console.Clear();
                    Console.WriteLine("Seed: {0}, #Samples: {1}", seed, mocompass.AllSolutions.Count);
                }
            }

            using (var sw = new System.IO.StreamWriter("results.csv"))
                foreach (var t in stats.Output) sw.WriteLine("{0},{1}", t.Item1, t.Item2);
        }    
    }
}
