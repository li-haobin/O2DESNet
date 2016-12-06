using MathNet.Numerics.LinearAlgebra.Double;
using O2DESNet.Optimizer;
using SingaPort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForMoCompassGo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("#Seeds: ");
            var nSeeds = Convert.ToInt32(Console.ReadLine());

            var stats = new AverageRunningStats(nSeeds);
            
            var samplingScheme = MoCompass.SamplingScheme.GoCS;
            var multiGradientScheme = MoCompass.MultiGradientScheme.Averaged;
            var pivotSelectionScheme = MoCompass.PivotSelectionScheme.MultiGradient;

            for (int seed = 0; seed < nSeeds; seed++)
            {
                int length = 1000;
                int batchSize = 1;
                
                var mocompass = new MoCompass(new ConvexSet(3, globalLb: 1), seed: seed, initialDynamicBound: 100);
                mocompass.SamplingSchemeOption = samplingScheme;
                mocompass.MultiGradientSchemeOption = multiGradientScheme;
                mocompass.PivotSelectionSchemeOption = pivotSelectionScheme;

                // initial set
                mocompass.Enter(new DenseVector[] { new double[] { 1, 1, 1 }, new double[] { 300, 300, 300 } }.Select(d =>
                {
                    var sol = InitialEvaluate(d, 0, 2);
                    return sol;
                }));

                while (mocompass.AllSolutions.Count < length)
                {
                    DenseVector[] decs;
                    decs = mocompass.Sample(batchSize, decimals: 0, nTrials: 10000);
                    if (decs.Length < 1) break;
                    mocompass.Enter(decs.Select(d =>
                    {
                        var sol = InitialEvaluate(d, 0, 2);
                        return sol;
                    }));
                    lock (stats)
                    {
                        stats.Log(seed, mocompass.AllSolutions.Count,
                            Pareto.DominatedHyperVolume(mocompass.ParetoSet.Select(s => s.Objectives), new double[] { 0, 3600 }));
                    }
                    Console.Clear();
                    Console.WriteLine("Seed: {0}, #Samples: {1}", seed, mocompass.AllSolutions.Count);
                    foreach (var sol in mocompass.ParetoSet.OrderBy(s => s.Objectives[0]))
                    {
                        Console.WriteLine("{0:F4}\t{1:F4}", sol.Objectives[0], sol.Objectives[1]);
                    }
                }
            }

            using (var sw = new System.IO.StreamWriter(
                string.Format("results_{0}_{1}_{2}.csv", samplingScheme, multiGradientScheme, pivotSelectionScheme)))
                foreach (var t in stats.Output) sw.WriteLine("{0},{1}", t.Item1, t.Item2);
        }

        public static StochasticSolution InitialEvaluate(DenseVector configs, int startSeed, int nSeeds)
        {
            DenseVector price = new double[] { 8, 3, 1 };

            var scenario = Scenario.GetPolderDesign(mTEUs: 12.1 / 1.5, nQC: (int)configs[0], nYC: (int)configs[1], nVehicle: (int)configs[2]);
            var observations = new DenseVector[nSeeds];
            var sumUniGradients = new DenseVector(new double[] { 0, 0, 0 });
            Parallel.ForEach(Enumerable.Range(0, nSeeds), i => {
                var sim = new SingaPort.SimpleModel.Simulator(new SingaPort.SimpleModel.Status(scenario, startSeed + i));
                sim.Run(TimeSpan.FromDays(365 * 1));
                observations[i] = new double[] { -sim.Status.BoARate }.Concat(new double[] { configs.DotProduct(price) }).ToArray();
                lock (scenario)
                {
                    sumUniGradients += new double[] {
                        - sim.Status.Utilization_QC.AverageCount,
                        - sim.Status.Utilization_YC.AverageCount,
                        - sim.Status.Utilization_Traffic.AverageCount,
                    };
                }
            });

            return new StochasticSolution(configs, observations) { Gradients = DenseMatrix.OfRowVectors(new DenseVector[] { sumUniGradients / nSeeds, price }) };
        }
    }
}
