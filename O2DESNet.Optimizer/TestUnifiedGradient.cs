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
            int length = 1000, nSeeds = 30;
            //Experiment(length, nSeeds, MoCompass.SamplingScheme.CoordinateSampling);
            //Experiment(length, nSeeds, MoCompass.SamplingScheme.GoCS, MoCompass.MultiGradientScheme.Unified);
            //Experiment(length, nSeeds, MoCompass.SamplingScheme.GoCS, MoCompass.MultiGradientScheme.Averaged);
            //Experiment(length, nSeeds, MoCompass.SamplingScheme.GoCS, MoCompass.MultiGradientScheme.Random);

            //Experiment(length, nSeeds, MoCompass.SamplingScheme.PolarUniform);
            Experiment(length, nSeeds, MoCompass.SamplingScheme.GoPolars, MoCompass.MultiGradientScheme.Unified);
            //Experiment(length, nSeeds, MoCompass.SamplingScheme.GoPolars, MoCompass.MultiGradientScheme.Averaged);
            //Experiment(length, nSeeds, MoCompass.SamplingScheme.GoPolars, MoCompass.MultiGradientScheme.Random);
        }    
        
        static void Experiment(int length, int nSeeds, 
            MoCompass.SamplingScheme samplingScheme, 
            MoCompass.MultiGradientScheme multiGradientScheme = MoCompass.MultiGradientScheme.Unified)
        {
            int dimension = 5;
            int batchSize = 4;
            var zdt = new Benchmarks.ZDT1(dimension);

            var stats = new AverageRunningStats(nSeeds);
            for (int seed = 0; seed < nSeeds; seed++)
            {
                var rs = new Random(seed);
                var mocompass = new MoCompass(zdt.DecisionSpace, seed: rs.Next());
                mocompass.SamplingSchemeOption = samplingScheme;
                mocompass.MultiGradientSchemeOption = multiGradientScheme;
                mocompass.PivotSelectionSchemeOption = MoCompass.PivotSelectionScheme.MultiGradient;//.Uniform;

                while (mocompass.AllSolutions.Count < length)
                {
                    DenseVector[] decs;
                    if (mocompass.ParetoSet.Length > 0) decs = mocompass.Sample(batchSize, decimals: 2, nTrials: 10000);
                    else decs = Enumerable.Range(0, batchSize).Select(i => (DenseVector)Enumerable.Range(0, dimension).Select(j => rs.NextDouble()).ToArray()).ToArray(); // randomize initial solutions
                    if (decs.Length < 1) break;
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

            using (var sw = new System.IO.StreamWriter(string.Format("results_{0}_{1}.csv", samplingScheme,
                (samplingScheme == MoCompass.SamplingScheme.GoCS || samplingScheme == MoCompass.SamplingScheme.GoPolars ?
                multiGradientScheme.ToString() : ""))))
                foreach (var t in stats.Output) sw.WriteLine("{0},{1}", t.Item1, t.Item2);
        }    
    }
}
