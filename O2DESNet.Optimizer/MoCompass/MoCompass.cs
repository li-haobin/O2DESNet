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
        internal SamplingScheme SamplingSchemeOption { get; private set; }
        private Random _rs;

        private HashSet<DenseVector> _paretoDecisions;
        public StochasticSolution[] ParetoSet { get { return _paretoDecisions.Select(d => AllSolutions[d]).ToArray(); } }
        public Dictionary<DenseVector, StochasticSolution> AllSolutions { get; private set; }
        
        public MoCompass(ConvexSet decisionSpace, SamplingScheme samplingScheme = SamplingScheme.CoordinateSampling, int seed = 0, double initialDynamicBound = 10)
        {
            DecisionSpace = new ConvexSet(decisionSpace.Dimension, constraints: decisionSpace.Constraints);
            initialDynamicBound = Math.Abs(initialDynamicBound);
            DynamicLowerBounds = DenseMatrix.CreateIdentity(DecisionSpace.Dimension).ToRowArrays().Select(coeffs => new ConstraintGE(coeffs, -initialDynamicBound)).ToArray();
            DynamicUpperBounds = DenseMatrix.CreateIdentity(DecisionSpace.Dimension).ToRowArrays().Select(coeffs => new ConstraintLE(coeffs, initialDynamicBound)).ToArray();
            DecisionSpace.Add(DynamicLowerBounds);
            DecisionSpace.Add(DynamicUpperBounds);

            SamplingSchemeOption = samplingScheme;
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
                    var crowdDistances = CrowdDistance.Calculate(_paretoDecisions.Select(d => AllSolutions[d].Objectives));
                    var p = TruncatedGeometric.Sample(_paretoDecisions.OrderByDescending(d => crowdDistances[AllSolutions[d].Objectives]), 0.2, _rs); // select pivot point
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
                // always applies the latest uniGradient information is it's available
                if (eval.UniGradient != null) AllSolutions[eval.Decisions].UniGradient = eval.UniGradient;
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
        }

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

        public enum SamplingScheme { CoordinateSampling, PolarUniform, GoPolars };
    }
}
