using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Replicators
{
    public class MinSelector<TScenario, TStatus, TSimulator> : Replicator<TScenario, TStatus, TSimulator>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TSimulator : Simulator<TScenario, TStatus>
    {
        public MinSelector(
            IEnumerable<TScenario> scenarios,
            Func<TScenario, int, TStatus> constrStatus,
            Func<TStatus, TSimulator> constrSimulator,
            Func<TStatus, bool> terminate,
            Func<TStatus, double> objective,
            double inDifferentZone = 0) :
            base(scenarios, constrStatus, constrSimulator, terminate, objective)
        { InDifferentZone = inDifferentZone; }

        private double _IDZConfidenceLevel = 0.99;
        public double InDifferentZone { get; private set; }
        public TScenario Optimum { get { return Statistics.Aggregate((s0, s1) => s0.Value.Mean <= s1.Value.Mean ? s0 : s1).Key; } }
        public TScenario[] InDifferentScenarios {
            get
            {
                var optimum = Optimum;
                return Scenarios.Where(sc => sc != optimum && ProbLessThan(
                    sc, Statistics[optimum].Mean, Statistics[optimum].Variance / Statistics[optimum].Count, InDifferentZone)
                    > _IDZConfidenceLevel).ToArray();
            }
        }

        private double ProbLessThan(TScenario sc, double mean, double sqrStdErr, double threshold)
        {
            var diff = Statistics[sc].Mean - mean;
            var err = Math.Sqrt(Statistics[sc].Variance / Statistics[sc].Count + sqrStdErr);
            return Normal.CDF(diff, err, threshold);
        }

        /// <summary>
        /// Probability of correct selection
        /// </summary>
        public double PCS {
            get
            {
                var optimum = Optimum;
                var minMean = Statistics[optimum].Mean;
                var sqrStdErr = Statistics[optimum].Variance / Statistics[optimum].Count;
                double pcs = 1.0;
                foreach (var sc in Scenarios.Except(InDifferentScenarios)) if (sc != optimum)
                        pcs *= 1 - ProbLessThan(sc, minMean, sqrStdErr, 0);
                return pcs;
            }
        }

        public void OCBAlloc(int budget)
        {
            var scenarios = Scenarios.Except(InDifferentScenarios).ToList();
            var ratios = OCBARatios(
                scenarios.Select(sc => Statistics[sc].Mean).ToArray(),
                scenarios.Select(sc => Statistics[sc].StandardDeviation).ToArray());
            Alloc(budget, Enumerable.Range(0, scenarios.Count).ToDictionary(i => scenarios[i], i => ratios[i]));
        }

        /// <summary>
        /// Get budget allocation ratios by to OCBA rule, given mean and sigma values for all conigurations
        /// </summary>
        private static double[] OCBARatios(double[] means, double[] sigmas)
        {
            var indices = Enumerable.Range(0, means.Count()).ToList();
            var min = means.Min();
            var minIndices = indices.Where(i => means[i] == min).ToArray();
            if (minIndices.Count() < indices.Count())
            {
                var ratios = indices.Select(i => Math.Pow(sigmas[i] / (means[i] - min), 2)).ToArray();
                foreach (var i in minIndices) ratios[i] = sigmas[i] * Math.Sqrt(indices.Except(minIndices).Sum(j => Math.Pow(ratios[j] / sigmas[j], 2)));
                var sum = ratios.Sum();
                return ratios.Select(r => r / sum).ToArray();
            }
            return Enumerable.Repeat(1.0 / indices.Count, indices.Count).ToArray();
        }

        public override void Display()
        {
            Scenarios.Sort((s1, s2) => Statistics[s2].Mean.CompareTo(Statistics[s1].Mean));

            Console.WriteLine("mean\tstddev\t#reps");
            foreach (var sc in Scenarios)
            {
                var stats = Statistics[sc];
                Console.Write("{0:F4}\t{1:F4}\t{2}\t", stats.Mean, stats.StandardDeviation, stats.Count);
                if (sc == Optimum) Console.Write("*");
                if (InDifferentScenarios.Contains(sc)) Console.Write("-");
                Console.WriteLine();
            }
            Console.WriteLine("------------------");
            Console.WriteLine("Total Budget:\t{0}", TotalBudget);
            Console.WriteLine("# Scenarios:\t{0}", Scenarios.Count);

            Console.WriteLine("PCS:\t{0:F4}", PCS);
        }
    }
}
