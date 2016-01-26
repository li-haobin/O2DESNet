using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
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
            Func<TStatus, double[]> objectives, // only the 1st objective is minimized
            double inDifferentZone = 0) :
            base(scenarios, constrStatus, constrSimulator, terminate, objectives)
        { InDifferentZone = inDifferentZone; }

        private double _IDZConfidenceLevel = 0.99;
        public double InDifferentZone { get; private set; }
        public TScenario Optimum
        {
            get
            {
                return Objectives.Aggregate((s0, s1) => GetObjectives(s0.Key, 0).Mean() <= GetObjectives(s1.Key, 0).Mean() ? s0 : s1).Key;
            }
        }
        public TScenario[] InDifferentScenarios {
            get
            {
                var optimum = Optimum;
                return Scenarios.Where(sc => sc != optimum && ProbLessThan(
                    sc, Objectives[optimum].Select(o => o[0]).Mean(), Objectives[optimum].Select(o => o[0]).Variance() / Objectives[optimum].Count, InDifferentZone)
                    > _IDZConfidenceLevel).ToArray();
            }
        }

        private double ProbLessThan(TScenario sc, double mean, double sqrStdErr, double threshold)
        {
            var diff = GetObjectives(sc, 0).Mean() - mean;
            var err = Math.Sqrt(GetObjectives(sc, 0).Variance() / Objectives[sc].Count + sqrStdErr);
            return Normal.CDF(diff, err, threshold);
        }

        /// <summary>
        /// Probability of correct selection
        /// </summary>
        public double PCS {
            get
            {
                var optimum = Optimum;
                var minMean = GetObjectives(optimum, 0).Mean();
                var sqrStdErr = GetObjectives(optimum, 0).Variance() / Objectives[optimum].Count;
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
                scenarios.Select(sc => GetObjectives(sc, 0).Mean()).ToArray(),
                scenarios.Select(sc => GetObjectives(sc, 0).StandardDeviation()).ToArray());
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
            Scenarios.Sort((s1, s2) => GetObjectives(s2, 0).Mean().CompareTo(GetObjectives(s1, 0).Mean()));

            Console.WriteLine("mean\tstddev\t#reps");
            foreach (var sc in Scenarios)
            {
                var objectives = GetObjectives(sc, 0);
                Console.Write("{0:F4}\t{1:F4}\t{2}\t", objectives.Mean(), objectives.StandardDeviation(), objectives.Count);
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
