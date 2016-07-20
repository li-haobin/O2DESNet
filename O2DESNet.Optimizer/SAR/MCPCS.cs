using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public class MCPCS : MonteCarloMyopicRule
    {
        public MCPCS(int k = 1000, int seed = 0) : base(k, seed) { }

        protected override double[] GetRatios(StochasticSolution[] solutions)
        {
            var pSet = new HashSet<int>(Pareto.GetParetoSet(Enumerable.Range(0, solutions.Length), i => solutions[i].Objectives));
            var popMeans = Enumerable.Range(0, K).Select(k => solutions.Select(s => s.PopMeans(RS)).ToList()).ToList();
            Func<HashSet<int>, int[], int> equal = (hs, arr) =>
            {
                if (hs.Count != arr.Length) return 0;
                foreach (var i in arr) if (!hs.Contains(i)) return 0;
                return 1;
            };
            double countCS = ParallelEnumerable.Range(0, K).Sum(k => equal(pSet, Pareto.GetParetoSet(Enumerable.Range(0, solutions.Length), i => popMeans[k][i])));

            int plus = 1;
            while (true)
            {
                var popMeansPlus = Enumerable.Range(0, K).Select(k => solutions.Select(s => s.PopMeans(RS, plus)).ToList()).ToList();
                var increments = Enumerable.Range(0, solutions.Length).Select(j => ParallelEnumerable.Range(0, K).Sum(k => equal(pSet, Pareto.GetParetoSet(Enumerable.Range(0, solutions.Length), i => i == j ? popMeansPlus[k][i] : popMeans[k][i]))) - countCS).ToArray();
                if (increments.Max() > 0) return increments;
                else plus *= 2;
                if (plus == 0) return solutions.Select(s => 1.0).ToArray();
            }
        }
    }
}
