using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace O2DESNet.Optimizer
{
    public class OCBAmcvr : MonteCarloMyopicRule
    {
        /// <param name="k">Monte Carlo sample size</param>
        /// <param name="seed">Random seed</param>
        public OCBAmcvr(int k = 1000, int seed = 0) : base(k, seed) { }

        // Variance Reductions
        protected override double[] GetRatios(StochasticSolution[] solutions)
        {
            int m = solutions.Length;
            var pm = new double[m, K]; // posterior means
            var pmi = new double[m, K]; // posterior means with incremental budget

            for (int i = 0; i < m; i++)
                for (int k = 0; k < K; k++)
                {
                    // consider only the 1st objective even if there are multiple
                    pm[i, k] = Normal.Sample(RS, solutions[i].Objectives[0], solutions[i].StandardDeviations[0] / Math.Sqrt(solutions[i].Observations.Count));
                    pmi[i, k] = Normal.Sample(RS, solutions[i].Objectives[0], solutions[i].StandardDeviations[0] / Math.Sqrt(solutions[i].Observations.Count + 1));
                }

            var variance = Enumerable.Range(0, K).Select(k => Enumerable.Range(0, m).Min(i => pm[i, k])).Variance();
            var reductions = Enumerable.Range(0, m)
                .Select(i0 => variance - Enumerable.Range(0, K)
                .Select(k => Enumerable.Range(0, m).Min(i => i == i0 ? pmi[i, k] : pm[i, k])).Variance())
                .ToArray();
            return reductions;
        }
    }
}
