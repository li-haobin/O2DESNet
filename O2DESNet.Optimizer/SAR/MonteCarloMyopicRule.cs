using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public abstract class MonteCarloMyopicRule : SAR
    {
        /// <summary>
        /// Monte Carlo sample size
        /// </summary>
        public int K { get; private set; }
        protected Random RS { get; private set; }
        
        protected MonteCarloMyopicRule(int k = 1000, int seed = 0)
        {
            K = k;
            RS = new Random(seed);
        }

        public override Dictionary<DenseVector, int> Alloc(int budget, IEnumerable<StochasticSolution> solutions)
        {
            var alloc = PreAlloc(ref budget, ref solutions);
            if (solutions.Count() < 1) return alloc;

            while (true)
            {
                var ratios = GetRatios(solutions.ToArray()).Select(r => Math.Max(0, r)).ToArray();
                if (ratios.Sum() > 0)
                {
                    var budgets = Divide(budget, ratios);
                    foreach (var i in Enumerable.Range(0, budgets.Length))
                        if (budgets[i] > 0) alloc.Add(solutions.ElementAt(i).Decisions, budgets[i]);
                    return alloc;
                }
            }
        }

        protected abstract double[] GetRatios(StochasticSolution[] solutions);
    }
}
