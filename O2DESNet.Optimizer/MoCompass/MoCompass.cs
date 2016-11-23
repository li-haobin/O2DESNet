using MathNet.Numerics.LinearAlgebra.Double;
using O2DESNet.Optimizer.Samplings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public class MoCompass
    {
        internal ConvexSet DecisionSpace { get; private set; }
        internal Dictionary<DenseVector, MostPromisingArea> MostPromisingAreas { get; private set; }
        internal ConstraintGE[] DynamicLowerBounds { get; set; }
        internal ConstraintLE[] DynamicUpperBounds { get; set; }
        internal SamplingScheme SamplingSchemeOption { get; set; } = SamplingScheme.CoordinateSampling;
        internal MultiGradientScheme MultiGradientSchemeOption { get; set; } = MultiGradientScheme.Unified;
        internal PivotSelectionScheme PivotSelectionSchemeOption { get; set; } = PivotSelectionScheme.Uniform;
        private Random _rs;

        private HashSet<DenseVector> _paretoDecisions;
        public StochasticSolution[] ParetoSet { get { return _paretoDecisions.Select(d => AllSolutions[d]).ToArray(); } }
        public Dictionary<DenseVector, StochasticSolution> AllSolutions { get; private set; }
        
        public MoCompass(ConvexSet decisionSpace, int seed = 0, double initialDynamicBound = 10)
        {
            DecisionSpace = new ConvexSet(decisionSpace.Dimension, constraints: decisionSpace.Constraints);
            initialDynamicBound = Math.Abs(initialDynamicBound);
            DynamicLowerBounds = DenseMatrix.CreateIdentity(DecisionSpace.Dimension).ToRowArrays().Select(coeffs => new ConstraintGE(coeffs, -initialDynamicBound)).ToArray();
            DynamicUpperBounds = DenseMatrix.CreateIdentity(DecisionSpace.Dimension).ToRowArrays().Select(coeffs => new ConstraintLE(coeffs, initialDynamicBound)).ToArray();
            DecisionSpace.Add(DynamicLowerBounds);
            DecisionSpace.Add(DynamicUpperBounds);
            
            _rs = new Random(seed);
            _paretoDecisions = new HashSet<DenseVector>();
            MostPromisingAreas = new Dictionary<DenseVector, MostPromisingArea>();
            AllSolutions = new Dictionary<DenseVector, StochasticSolution>();
        }

        public DenseVector[] Sample(int batchSize, int decimals = 15, int nTrials = 100000)
        {
            int countTrials = 0;
            var samples = new HashSet<DenseVector>();
            Func<DenseVector, DenseVector> round = point => point.Select(v => Math.Round(v, decimals)).ToArray();

            if (_paretoDecisions.Count < 1)
            {
                // initial batch
                samples.Add(round(GetFeasibleDecisions()));
                while (samples.Count < batchSize && ++countTrials < nTrials)
                {
                    var sample = round(new MostPromisingArea(this, samples.Last()).Sample(SamplingScheme.CoordinateSampling, _rs));
                    if (!samples.Contains(sample)) samples.Add(sample);
                }
            }
            else {
                // subsequent batches
                while (samples.Count < batchSize && ++countTrials < nTrials)
                {
                    DenseVector p = null;
                    
                    switch (PivotSelectionSchemeOption)
                    {
                        case PivotSelectionScheme.Uniform:
                            p = _paretoDecisions.ElementAt(_rs.Next(_paretoDecisions.Count));
                            break;
                        case PivotSelectionScheme.CrowdDistance:
                            var crowdDistances = CrowdDistance.Calculate(_paretoDecisions.Select(d => AllSolutions[d].Objectives));
                            p = TruncatedGeometric.Sample(_paretoDecisions.OrderByDescending(d => crowdDistances[AllSolutions[d].Objectives]), 0.2, _rs);
                            break;
                        case PivotSelectionScheme.MultiGradient:
                            p = TruncatedGeometric.Sample(_paretoDecisions.OrderByDescending(d => UnifiedGradient[d].L2Norm()), 0.2, _rs);
                            break;
                    }

                    var mpa = MostPromisingAreas[p]; // sample an MPA
                    var sample = round(mpa.Sample(SamplingSchemeOption, _rs)); // sample a point in the MPA
                    if (!samples.Contains(sample) && !AllSolutions.ContainsKey(sample)) samples.Add(sample);
                }
            }

            return samples.ToArray();
        }

        public void Enter(StochasticSolution evaluation, bool append = true) { Enter(new StochasticSolution[] { evaluation }, append); }
        public void Enter(IEnumerable<StochasticSolution> evaluations, bool append = true)
        {
            foreach (var d in evaluations.Select(e => e.Decisions)) RelaxDynamicBounds(d);
            evaluations = evaluations.Where(e => DecisionSpace.Contains(e.Decisions));

            // form the stochastic solution
            foreach (var eval in evaluations)
            {
                StochasticSolution solution;
                if (AllSolutions.ContainsKey(eval.Decisions))
                {
                    if (append)
                    {
                        solution = AllSolutions[eval.Decisions];
                        solution.Evaluate(eval.Observations);
                    }
                    else {
                        solution = new StochasticSolution(eval.Decisions, eval.Observations);
                        AllSolutions[eval.Decisions] = solution;
                    }
                }
                else { solution = new StochasticSolution(eval.Decisions, eval.Observations); AllSolutions.Add(eval.Decisions, solution); }
                // always applies the latest Gradients information if it's available
                if (eval.Gradients != null) AllSolutions[eval.Decisions].Gradients = eval.Gradients;
            }

            // identify new Pareto set
            var newParetoDecisions = new HashSet<DenseVector>(Pareto.GetParetoSet(
                _paretoDecisions.Concat(evaluations.Select(e => e.Decisions)).Distinct(),
                d => AllSolutions[d].Objectives));

            // update for current evaluations
            var allDominated = AllSolutions.Keys.Except(newParetoDecisions);
            foreach (var d in evaluations.Select(e => e.Decisions).Distinct())
            {
                if (!_paretoDecisions.Contains(d) && newParetoDecisions.Contains(d))
                {
                    MostPromisingAreas.Add(d, new MostPromisingArea(this, d, allDominated));
                    RelaxDynamicBounds(d);
                }
                else if (!newParetoDecisions.Contains(d)) foreach (var p in _paretoDecisions) MostPromisingAreas[p].Add(d);
            }

            // update for new dominated solutions
            foreach (var d in _paretoDecisions.Except(newParetoDecisions))
            {
                MostPromisingAreas.Remove(d);
                foreach (var p in newParetoDecisions) MostPromisingAreas[p].Add(d);
            }

            _paretoDecisions = newParetoDecisions;

            if (SamplingSchemeOption == SamplingScheme.GoPolars || SamplingSchemeOption == SamplingScheme.GoCS)
            {
                var paretoSet = ParetoSet;
                var weightingVectors = Pareto.UnifiedWeightingVectors(AllSolutions.Select(s => s.Value.Objectives));

                switch (MultiGradientSchemeOption)
                {
                    case MultiGradientScheme.Unified:
                        UnifiedGradient = paretoSet.ToDictionary(s => s.Decisions, s => weightingVectors[s.Objectives] * s.Gradients);
                        break;
                    case MultiGradientScheme.Averaged:
                        UnifiedGradient = paretoSet.ToDictionary(s => s.Decisions, s => Enumerable.Range(0, s.Gradients.RowCount).Select(i => 1.0).ToArray() * s.Gradients);
                        break;
                    case MultiGradientScheme.Random:
                        UnifiedGradient = paretoSet.ToDictionary(s => s.Decisions, s => (DenseVector)s.Gradients.ToRowArrays()[_rs.Next(s.Gradients.RowCount)]);
                        break;
                    case MultiGradientScheme.MinNorm:
                        UnifiedGradient = paretoSet.ToDictionary(s => s.Decisions, s => (DenseVector)(MinNormCoefficients(s.Gradients.ToRowArrays()).ToRowMatrix() * s.Gradients).ToRowArrays()[0]);
                        break;
                }
            }
        }
        internal Dictionary<DenseVector, DenseVector> UnifiedGradient { get; private set; }

        #region For MiniNormCoefficients Calculation
        private static DenseVector MinNormCoefficients(double[][] points)
        {
            if (points.Length == 2) return MinNormCoefficients_2D(points[0], points[1]);
            Func<double[][], DenseVector, double> norm = (pts, a) =>
            {
                var l = a.Sum();
                if (l == 0) return double.PositiveInfinity;
                a = a / l;
                return (DenseMatrix.OfRowVectors(a) * DenseMatrix.OfRowArrays(pts)).ToRowWiseArray().Sum(v => v * v);
                //return Math.Sqrt(pts.Select(p => ((DenseVector)p).DotProduct(a)).Sum(v => v * v));
            };

            var indices = Enumerable.Range(0, points.Length);

            var mc = new MoCompass(new ConvexSet(points.Length, globalLb: 0, globalUb: 1));
            while (mc.AllSolutions.Count < 3000)
            {
                var decs = mc.Sample(1);
                if (decs.Length < 1) break;
                mc.Enter(decs.Select(d => new StochasticSolution(d, new double[] { norm(points, d) })));
            }

            var coeffs = new DenseVector(mc.ParetoSet.First().Decisions.Take(points.Length).ToArray());
            //var n = norm(points, coeffs);
            //var n1 = norm(points, new double[] { 0.5, 0.5 });
            return coeffs / coeffs.L2Norm();
        }
        /// Giacomini, Matteo (2013), Multiple-Gradient Descent Algorithm for isogeometric shape optimization, P.17
        private static DenseVector MinNormCoefficients_2D(DenseVector v, DenseVector w)
        {
            double vNorm = v.L2Norm(), wNorm = w.L2Norm();
            if (v.DotProduct(w) < Math.Pow(Math.Min(vNorm, wNorm), 2))
            {
                var gamma = w.DotProduct(w - v) / (v - w).Sum(u => u * u);
                return new double[] { gamma, 1 - gamma };
            }
            if (vNorm < wNorm) return new double[] { 1, 0 };
            return new double[] { 0, 1 };
        }
        #endregion

        private DenseVector GetFeasibleDecisions()
        {
            var point = DenseVector.Create(DecisionSpace.Dimension, 0);
            var constraints = DecisionSpace.Constraints.Except(DynamicLowerBounds).Except(DynamicUpperBounds).ToList();
            if (constraints.Count == 0) return point;

            var phaseI = new MoCompass(new ConvexSet(DecisionSpace.Dimension));
            while (true)
            {
                var d = phaseI.Sample(1).FirstOrDefault();
                if (d == null) throw new Exception("No feasible solution found.");
                var totalExcess = constraints.Select(c => Math.Max(0, -c.Slack(d))).Sum();
                if (totalExcess == 0) { RelaxDynamicBounds(d); return d; }
                phaseI.Enter(new StochasticSolution(d, new double[] { totalExcess }));
            }
        }

        /// <summary>
        /// Relax the dynamic bounds, such that the given point lies in its half range
        /// </summary>
        private void RelaxDynamicBounds(DenseVector point)
        {
            foreach (var i in Enumerable.Range(0, DecisionSpace.Dimension))
            {
                while (point[i] > DynamicUpperBounds[i].UpperBound / 2) DynamicUpperBounds[i].UpperBound = Math.Max(point[i], DynamicUpperBounds[i].UpperBound) * 2;
                while (point[i] < DynamicLowerBounds[i].LowerBound / 2) DynamicLowerBounds[i].LowerBound = Math.Min(point[i], DynamicLowerBounds[i].LowerBound) * 2;
            }
        }

        public enum PivotSelectionScheme { Uniform, CrowdDistance, MultiGradient }
        public enum SamplingScheme { CoordinateSampling, PolarUniform, GoPolars, GoCS };
        public enum MultiGradientScheme { Unified, Averaged, Random, MinNorm }
    }
}
