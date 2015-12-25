using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            bool parallelized = true) :
            base(scenarios, constrStatus, constrSimulator, terminate, objective, parallelized)
        { }

        public TScenario Optimum { get { return Statistics.Aggregate((s0, s1) => s0.Value.Mean <= s1.Value.Mean ? s0 : s1).Key; } }
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
                foreach (var sc in Scenarios) if (sc != optimum)
                    {
                        var diff = Statistics[sc].Mean - minMean;
                        var err = Math.Sqrt(Statistics[sc].Variance / Statistics[sc].Count + sqrStdErr);
                        pcs *= 1 - Normal.CDF(diff, err, 0);
                    }
                return pcs;
            }
        }

        public void OCBAlloc(int budget)
        {
            var ratios = OCBARatios(
                Scenarios.Select(sc => Statistics[sc].Mean).ToArray(),
                Scenarios.Select(sc => Statistics[sc].StandardDeviation).ToArray());
            Alloc(budget, Enumerable.Range(0, Scenarios.Count).ToDictionary(i => Scenarios[i], i => ratios[i]));
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
    }
}
