using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer
{
    public class OCBA : SAR
    {
        public override Dictionary<DenseVector, int> Alloc(int budget, IEnumerable<StochasticSolution> solutions)
        {
            // consider only the 1st objective even if there are multiple
            return Alloc(budget, solutions, 
                sols => GetTargetRatios(sols.Select(s => s.Objectives[0]).ToArray(), sols.Select(s => s.StandardDeviations[0]).ToArray()));   
        }

        /// <summary>
        /// Get budget allocation ratios by to OCBA rule, given mean and sigma values for all designs
        /// </summary>
        private static double[] GetTargetRatios(double[] means, double[] sigmas)
        {
            for (int i = 0; i < sigmas.Length; i++) if (sigmas[i] == 0) sigmas[i] = 1E-7;
            var indices = Enumerable.Range(0, means.Length).ToArray();
            var min = means.Min();
            var minIndices = indices.Where(i => means[i] == min).ToArray();
            if (minIndices.Length < indices.Length)
            {
                var ratios = indices.Select(i => Math.Pow(sigmas[i] / (means[i] - min), 2)).ToArray();
                foreach (var i in minIndices) ratios[i] = sigmas[i] * Math.Sqrt(indices.Except(minIndices).Sum(j => Math.Pow(ratios[j] / sigmas[j], 2)));
                var sum = ratios.Sum();
                return ratios.Select(r => r / sum).ToArray();
            }
            // all equal
            return Enumerable.Repeat(1.0 / indices.Length, indices.Length).ToArray();
        }
    }
}
