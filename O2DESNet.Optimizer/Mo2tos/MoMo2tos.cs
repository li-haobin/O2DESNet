using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    /// <summary>
    /// According to MO-MO2TOS in Li et al. (2015) Winter Simulation Conference
    /// </summary>
    public class MoMo2tos
    {
        public Dictionary<DenseVector, TwoFidelitySolution> Solutions { get; private set; }
        public List<List<TwoFidelitySolution>> Groups { get; private set; }
        private DenseVector _worstPoint;
        private Random _rs;

        public MoMo2tos(IEnumerable<TwoFidelitySolution> solutions, int nGroups, DenseVector worstPoint = null, int seed = 0)
        {
            Solutions = solutions.ToDictionary(s => s.Decisions, s => s);
            Groups = Group(OrdinalTransform(Solutions.Values), nGroups);
            _rs = new Random(seed);
            _worstPoint = worstPoint;
            if (_worstPoint == null)
            {
                var objs = Solutions.Values.Select(s => s.LowFidelityObjectives).ToArray();
                _worstPoint = Enumerable.Range(0, objs[0].Count).Select(i => objs.Max(o => o[i])).ToArray();
            }
        }

        protected virtual List<TwoFidelitySolution> OrdinalTransform(IEnumerable<TwoFidelitySolution> solutions)
        {
            return Pareto.NonDominatedSort(solutions, s => s.LowFidelityObjectives).ToList();
        }

        protected List<List<TwoFidelitySolution>> Group(List<TwoFidelitySolution> solutions, int nGroups)
        {
            var groups = new List<List<TwoFidelitySolution>>();
            for (int i = 0; i < nGroups; i++)
            {
                int count = solutions.Count / (nGroups - i);
                groups.Add(solutions.Take(count).ToList());
                solutions.RemoveRange(0, count);
            }
            return groups;
        }

        public DenseVector Sample(double pGroup, double pSolution)
        {
            if (pGroup == 0 && pSolution == 0)
            {
                while (true)
                {
                    var s = Solutions.Values.ElementAt(_rs.Next(Solutions.Count));
                    if (s.Objectives == null) return s.Decisions;
                }
            }
            var group = Groups.Where(g => g.Count(s => s.Objectives != null) < 1).FirstOrDefault();
            if (group == null) group = 
                    Samplings.TruncatedGeometric.Sample(
                        Groups.Where(g => g.Count(s => s.Objectives == null) > 0), 
                        pGroup, _rs);
            return Samplings.TruncatedGeometric.Sample(group.Where(s => s.Objectives == null), pSolution, _rs).Decisions;
        }

        public void Evaluate(DenseVector decisions, DenseVector highFidelityObjectives)
        {
            if (!Solutions.ContainsKey(decisions)) throw new Exception("Solution does not exist.");
            Solutions[decisions].Evaluate(highFidelityObjectives);
            ReorderGroups();
        }

        private void ReorderGroups()
        {
            if (Groups.Count(g => g.Count(s => s.Objectives != null) < 1) > 0) return;
            var sampled = Pareto.NonDominatedSort(Solutions.Values.Where(s => s.Objectives != null), s => s.Objectives).ToList();
            var hfRanks = Enumerable.Range(0, sampled.Count).ToDictionary(i => sampled[i], i => (double)i);
            Groups = Groups.OrderBy(g => g.Where(s => s.Objectives != null).Average(s => hfRanks[s])).ToList();
        }

        public List<TwoFidelitySolution> EvaluatedSet
        {
            get { return Solutions.Values.Where(s => s.Objectives != null).ToList(); }
        }
        public double DHV
        {
            get
            {
                return Pareto.DominatedHyperVolume(
                    Solutions.Values.Where(s => s.Objectives != null).Select(s => s.Objectives), 
                    _worstPoint);
            }
        }
    }
    
}
