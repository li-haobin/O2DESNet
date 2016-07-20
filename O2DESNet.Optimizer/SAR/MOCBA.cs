using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;

namespace O2DESNet.Optimizer
{
    public class MOCBA : SAR
    {
        public override Dictionary<DenseVector, int> Alloc(int budget, IEnumerable<StochasticSolution> solutions)
        {
            return Alloc(budget, solutions, sols => new Calculation(sols).Ratios);
        }

        public double APCS_M(IEnumerable<StochasticSolution> solutions)
        {
            var h = solutions.First().Objectives.Count;
            var k = solutions.Count();
            var sp = Pareto.GetParetoSet(solutions, s => s.Objectives).ToList();
            var spc = solutions.Except(sp).ToList();
            var ub1 = h * spc.Count - h * spc.Sum(i => solutions.Where(j => i != j).Max(j => Enumerable.Range(0, h).Min(l => StochasticSolution.ProbLE(i, j, l))));
            var ub2 = (k - 1) * sp.Sum(i => solutions.Where(j => i != j).Max(j => Enumerable.Range(0, h).Min(l => StochasticSolution.ProbLE(i, j, l))));
            return 1 - ub1 - ub2;
        }        

        private class Calculation
        {
            private StochasticSolution[] Solutions { get; set; }
            internal double[] Ratios { get; private set; }
            internal Calculation(IEnumerable<StochasticSolution> solutions)
            {
                Solutions = solutions.ToArray();
                var ratios = new Dictionary<StochasticSolution, double>();
                if (Solutions.Length > 1)
                {
                    foreach (var h in SA)
                    {
                        var j = GetJ(h);
                        var l = GetL(h, j);
                        ratios.Add(h, Math.Pow(Sigma(h, l) / Delta(h, j, l), 2));
                    }
                    foreach (var d in SB)
                    {
                        double sum = 0;
                        foreach (var h in Theta_d(d))
                        {
                            var l = GetL(h, d);
                            sum += Math.Pow(ratios[h] * Sigma(d, l) / Sigma(h, l), 2);
                        }
                        ratios.Add(d, Math.Sqrt(sum));
                    }
                }
                else { foreach (var s in Solutions) ratios.Add(s, 1); }
                Ratios = Solutions.Select(s => ratios[s]).ToArray();
            }

            private double Sigma(StochasticSolution i, int l) { return Math.Max(1E-7, i.StandardDeviations[l]); }

            //Eq6.40
            private double Delta(StochasticSolution i, StochasticSolution j, int l) { return j.Objectives[l] - i.Objectives[l]; }

            private double GetInnerValue(StochasticSolution i, StochasticSolution j, int l)
            {
                var delta_ijl = Delta(i, j, l);
                var sigma_il = Sigma(i, l);
                var sigma_jl = Sigma(j, l);
                return delta_ijl * Math.Abs(delta_ijl) / (sigma_il * sigma_il + sigma_jl * sigma_jl);
            }

            //Eq6.41
            private int GetL(StochasticSolution i, StochasticSolution j)
            {
                List<double> values = new List<double>();
                for (int l = 0; l < i.Objectives.Count; l++) values.Add(GetInnerValue(i, j, l));
                return Enumerable.Range(0, values.Count).Aggregate((i1, i2) => values[i1] > values[i2] ? i1 : i2);
                //return values.IndexOf(values.Max());
            }
            
            //Eq6.42
            private Dictionary<StochasticSolution, StochasticSolution> JTable = new Dictionary<StochasticSolution, StochasticSolution>();
            private StochasticSolution GetJ(StochasticSolution i)
            {
                if (!JTable.ContainsKey(i))
                {
                    var pairs = Solutions.Where(s => s != i).Select(s => new { Solution = s, Value = GetInnerValue(i, s, GetL(i, s)) }).ToList();
                    JTable.Add(i, pairs.OrderBy(p => p.Value).First().Solution);
                }
                return JTable[i];
            }
            
            //Eq6.43
            private List<StochasticSolution> _sA = null;
            private List<StochasticSolution> SA
            {
                get
                {
                    if (_sA == null)
                    {
                        _sA = new List<StochasticSolution>();
                        foreach (var h in Solutions)
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
            private List<StochasticSolution> _sB = null;
            private List<StochasticSolution> SB { get { if (_sB == null) _sB = Solutions.Except(SA).ToList(); return _sB; } }

            //Eq6.45-1
            private Dictionary<StochasticSolution, List<StochasticSolution>> _theta_hTable = new Dictionary<StochasticSolution, List<StochasticSolution>>();
            private List<StochasticSolution> _theta_h(StochasticSolution h)
            {
                if (!_theta_hTable.ContainsKey(h))
                {
                    _theta_hTable.Add(h, Solutions.Where(i => GetJ(i).Equals(h)).ToList());
                }
                return _theta_hTable[h];
            }

            //Eq6.45-2
            private Dictionary<StochasticSolution, List<StochasticSolution>> Theta_dTable = new Dictionary<StochasticSolution, List<StochasticSolution>>();
            private List<StochasticSolution> Theta_d(StochasticSolution d)
            {
                if (!Theta_dTable.ContainsKey(d)) Theta_dTable.Add(d, SA.Where(h => GetJ(h) == d).ToList());
                return Theta_dTable[d];
            }
        }
    }
}
