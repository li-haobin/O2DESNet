using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public abstract class SAR
    {
        public abstract Dictionary<DenseVector, int> Alloc(int budget, IEnumerable<StochasticSolution> solutions);

        /// <summary>
        /// Assign marginal budget according to the target ratios, considering budgets have been assgined.
        /// </summary>
        private static int[] Assign(int budget, DenseVector targetRatios, int[] assigned = null)
        {
            if (assigned == null) return Divide(budget, targetRatios);
            targetRatios /= targetRatios.Sum();
            var totalBudget = budget + assigned.Sum();
            return Divide(budget, Enumerable.Range(0, targetRatios.Count).Select(i => Math.Max(0, totalBudget * targetRatios[i] - assigned[i])).ToArray());
        }

        /// <summary>
        /// Divide a given budget according to the ratios
        /// </summary>
        protected static int[] Divide(int budget, DenseVector ratios)
        {
            double sum = ratios.Sum(), cum = 0;
            int assigned = 0;
            var divide = Enumerable.Repeat(0, ratios.Count).ToArray();
            var indices = Enumerable.Range(0, ratios.Count).OrderBy(i => ratios[i]).ToArray();
            for (int i = 0; i < ratios.Count; i++)
            {
                cum += ratios[indices[i]];
                divide[indices[i]] = (int)Math.Floor(budget * cum / sum) - assigned;
                assigned += divide[indices[i]];
            }
            return divide;
        }

        /// <summary>
        /// Get allocation given budget, candidate solutions, and rules
        /// </summary>
        /// <param name="budget">budget to allocate</param>
        /// <param name="solutions">candidate solutions</param>
        /// <param name="getTargetRatios">the allocation rules on distinct and replicated solutions</param>
        protected Dictionary<DenseVector, int> Alloc(int budget, IEnumerable<StochasticSolution> solutions, Func<StochasticSolution[],double[]> getTargetRatios)
        {
            // Pre-allocation
            var alloc = PreAlloc(ref budget, ref solutions);
            if (solutions.Count() < 1) return alloc;

            // allocate the remaining budget for the replcated solutions
            var targetRatios = getTargetRatios(solutions.ToArray());
            // in case some ratios are infinity, bring them down to the max finite values
            // this may happen when identical value appears at certain objective, and indifferent-zone is not considered
            if (double.IsInfinity(targetRatios.Max()))
            {
                if (targetRatios.Count(r => r < double.PositiveInfinity) < 1) targetRatios = targetRatios.Select(r => 1.0).ToArray();
                var maxFinite = targetRatios.Where(r => !double.IsInfinity(r)).Max();
                targetRatios = targetRatios.Select(r => double.IsInfinity(r) ? maxFinite : r).ToArray();
            }
            var assignment = Assign(budget, targetRatios, solutions.Select(s => s.Observations.Count).ToArray());
            for (int i = 0; i < solutions.Count(); i++)
                if (assignment[i] > 0) alloc.Add(solutions.ElementAt(i).Decisions, assignment[i]);

            return alloc;
        }
        
        protected Dictionary<DenseVector, int> PreAlloc(ref int budget, ref IEnumerable<StochasticSolution> solutions)
        {
            var alloc = new Dictionary<DenseVector, int>();
            var solutionDict = new Dictionary<DenseVector, StochasticSolution>();
            foreach (var solution in solutions)
            {
                if (!solutionDict.ContainsKey(solution.Decisions))
                    solutionDict.Add(solution.Decisions, new StochasticSolution(solution.Decisions, solution.Observations));
                // if multiple solutions with identical design (decisions), treat them a single solution with aggregated observations
                else solutionDict[solution.Decisions].Evaluate(solution.Observations);
            }

            // first allocate to non-replicated solutions (with less than 2 observations, where stddev not applicable)
            var replicated = solutionDict.Values.Where(s => s.StandardDeviations != null).ToArray();
            foreach (var s in solutionDict.Values.Except(replicated))
            {
                int n = Math.Min(budget, 2 - s.Observations.Count);
                alloc.Add(s.Decisions, n);
                budget -= n;
            }
            solutions = replicated;
            return alloc;
        }
    }
}
