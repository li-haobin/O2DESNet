using MathNet.Numerics.Statistics;
using O2DESNet.MultiObjective;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Replicators
{
    public class ParetoFinder<TScenario, TStatus, TSimulator> : Replicator<TScenario, TStatus, TSimulator>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TSimulator : Simulator<TScenario, TStatus>
    {
        public ParetoFinder(
            IEnumerable<TScenario> scenarios,
            Func<TScenario, int, TStatus> constrStatus,
            Func<TStatus, TSimulator> constrSimulator,
            Func<TStatus, bool> terminate,
            Func<TStatus, double[]> objectives //, double inDifferentZone = 0
            ) :
            base(scenarios, constrStatus, constrSimulator, terminate, objectives)
        {
        }

        public TScenario[] Optima
        {
            get
            {
                var nObjs = Objectives.Values.First().First().Length;
                return ParetoOptimality.GetParetoSet<TScenario>(Scenarios.ToArray(),
                    (s1, s2) => ParetoOptimality.Dominate(
                        Enumerable.Range(0, nObjs).Select(l => GetObjectives(s1, l).Mean()).ToArray(),
                        Enumerable.Range(0, nObjs).Select(l => GetObjectives(s2, l).Mean()).ToArray()));
            }
        }

        public void MOCBAlloc(int budget) { Alloc(budget, MOCBARatios); }

        #region Methods for MOCBA 
        private Dictionary<TScenario, double> MOCBARatios
        {
            get
            {
                var budgetAllocation = new Dictionary<TScenario, double>();
                if (Scenarios.Count > 1)
                {
                    foreach (var h in SA)
                    {
                        var j = GetJ(h);
                        var l = GetL(h, j);
                        budgetAllocation.Add(h, Math.Pow(GetObjectives(h, l).StandardDeviation() / Delta(h, j, l), 2));
                    }
                    foreach (var d in SB)
                    {
                        double sum = 0;
                        foreach (var h in Theta_d(d))
                        {
                            var l = GetL(h, d);
                            sum += Math.Pow(budgetAllocation[h] * GetObjectives(d, l).StandardDeviation() / GetObjectives(h, l).StandardDeviation(), 2);
                        }
                        budgetAllocation.Add(d, Math.Sqrt(sum));
                    }
                }
                else { foreach (var s in Scenarios) budgetAllocation.Add(s, 1); }
                return budgetAllocation;
            }
        }       

        //Eq6.40
        private double Delta(TScenario i, TScenario j, int l)
        {
            return GetObjectives(j, l).Mean() - GetObjectives(i, l).Mean();
        }

        private double GetInnerValue(TScenario i, TScenario j, int l)
        {
            var delta_ijl = Delta(i, j, l);
            var sigma_il = GetObjectives(i, l).StandardDeviation();
            var sigma_jl = GetObjectives(j, l).StandardDeviation();
            return delta_ijl * Math.Abs(delta_ijl) / (sigma_il * sigma_il + sigma_jl * sigma_jl);
        }

        //Eq6.41
        private int GetL(TScenario i, TScenario j)
        {
            List<double> values = new List<double>();
            for (int l = 0; l < Objectives[i].First().Length; l++) values.Add(GetInnerValue(i, j, l));
            return Enumerable.Range(0, values.Count).Aggregate((i1, i2) => values[i1] > values[i2] ? i1 : i2);
            //return values.IndexOf(values.Max());
        }

        //static int l(Solution i, Solution j) // j dominates i
        //{
        //    List<double> probabilities = new List<double>();
        //    for (int l = 0; l < i.ObjectiveSampleMeans.Count; l++) probabilities.Add(Probability_CorrectComparison(i, j, l));
        //    return probabilities.IndexOf(probabilities.Min());
        //}

        //Eq6.42
        private Dictionary<TScenario, TScenario> JTable = new Dictionary<TScenario, TScenario>();
        private TScenario GetJ(TScenario i)
        {
            if (!JTable.ContainsKey(i))
            {
                var pairs = Scenarios.Where(s => s != i).Select(s => new { Solution = s, Value = GetInnerValue(i, s, GetL(i, s)) }).ToList();
                JTable.Add(i, pairs.OrderBy(p => p.Value).First().Solution);
            }
            return JTable[i];
        }

        // j_i
        //public static Solution j(Solution i, List<Solution> theta) // j dominates i
        //{
        //    theta = theta.Except(new List<Solution> { i }).ToList();
        //    var probabilities = theta.Select(j => Probability_Dominance(i, j)).ToList();
        //    return theta[probabilities.IndexOf(probabilities.Max())];
        //}

        //static double Probability_CorrectComparison(Solution i, Solution j, int l) // j_l <= i_l
        //{
        //    return 1 - MathNet.Numerics.Distributions.Normal.CDF(0,
        //        Math.Sqrt(Math.Pow(j.ObjectiveStandardDeviations[l], 2) + Math.Pow(i.ObjectiveStandardDeviations[l], 2)),
        //        delta(i, j, l));
        //}

        //public static double Probability_Dominance(Solution i, Solution j) // j dominate i
        //{
        //    double p = 1.0;
        //    for (int l = 0; l < i.ObjectiveSampleMeans.Count; l++) p *= Probability_CorrectComparison(i, j, l);
        //    return p;
        //}

        //Eq6.43
        private List<TScenario> _sA = null;
        private List<TScenario> SA
        {
            get
            {
                if (_sA == null)
                {
                    _sA = new List<TScenario>();
                    foreach (var h in Scenarios)
                    {
                        var j_h = GetJ(h);
                        var theta_h = _theta_h(h);
                        if (theta_h.Count < 1 || Math.Abs(GetInnerValue(h, j_h, GetL(h, j_h))) < theta_h.Min(i => Math.Abs(GetInnerValue(i, h, GetL(i, h))))) _sA.Add(h);
                    }
                }
                return _sA;
            }
        }

        //Eq6.44
        private List<TScenario> _sB = null;
        private List<TScenario> SB { get { if (_sB == null) _sB = Scenarios.Except(SA).ToList(); return _sB; } }

        //Eq6.45-1
        private Dictionary<TScenario, List<TScenario>> _theta_hTable = new Dictionary<TScenario, List<TScenario>>();
        private List<TScenario> _theta_h(TScenario h)
        {
            if (!_theta_hTable.ContainsKey(h))
            {
                _theta_hTable.Add(h, Scenarios.Where(i => GetJ(i).Equals(h)).ToList());
            }
            return _theta_hTable[h];
        }

        //Eq6.45-2
        private Dictionary<TScenario, List<TScenario>> Theta_dTable = new Dictionary<TScenario, List<TScenario>>();
        private List<TScenario> Theta_d(TScenario d)
        {
            if (!Theta_dTable.ContainsKey(d)) Theta_dTable.Add(d, SA.Where(h => GetJ(h) == d).ToList());
            return Theta_dTable[d];
        }
        #endregion
    }
}
