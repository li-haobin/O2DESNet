using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Replicators
{
    public class MCVR<TScenario, TStatus, TSimulator> : Replicator<TScenario, TStatus, TSimulator>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TSimulator : Simulator<TScenario, TStatus>
    {
        public MCVR(
            IEnumerable<TScenario> scenarios,
            Func<TScenario, int, TStatus> constrStatus,
            Func<TStatus, TSimulator> constrSimulator,
            Func<TStatus, bool> terminate,
            Func<TStatus, double> objective) :
            base(scenarios, constrStatus, constrSimulator, terminate, objective)
        { }

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
