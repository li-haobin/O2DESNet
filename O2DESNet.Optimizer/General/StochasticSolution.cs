using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public class StochasticSolution : Solution
    {
        public List<DenseVector> Observations { get; private set; }
        /// <summary>
        /// Standard deviations for each objective observation
        /// </summary>
        public DenseVector StandardDeviations { get; private set; }

        public StochasticSolution(DenseVector decisions, DenseVector observation = null) : base(decisions)
        {
            Observations = new List<DenseVector>();
            StandardDeviations = null;
            if (observation != null) Evaluate(observation);
        }
        public StochasticSolution(DenseVector decisions, IEnumerable<DenseVector> observations) : base(decisions)
        {
            Observations = new List<DenseVector>();
            StandardDeviations = null;
            if (observations != null && observations.Count() > 0) Evaluate(observations);
        }

        /// <summary>
        /// Add a new observation for the objective values
        /// </summary>
        public override void Evaluate(DenseVector observation) { Evaluate(new DenseVector[] { observation }); }
        /// <summary>
        /// Add multiple new observations for the objective values
        /// </summary>
        public void Evaluate(IEnumerable<DenseVector> observations)
        {
            foreach (var observation in observations)
            {
                if (Observations.Count > 0 && Observations[0].Count != observation.Count) throw new Exception_InconsistentDimensions();
                Observations.Add(observation);
            }
            Objectives = Enumerable.Range(0, Observations[0].Count).Select(i => Observations.Select(o => o[i]).Mean()).ToArray();
            if (Observations.Count > 1) StandardDeviations = Enumerable.Range(0, Observations[0].Count).Select(i => Observations.Select(o => o[i]).StandardDeviation()).ToArray();
        }

        /// <summary>
        /// Sample population means based on current observations, and budget increment
        /// </summary>
        public DenseVector PopMeans(Random rs, int budgetIncrement = 0)
        {
            if (StandardDeviations == null) return Objectives;
            return Enumerable.Range(0, Objectives.Count)
                .Select(i => Normal.Sample(rs, Objectives[i],
                StandardDeviations[i] / Math.Sqrt(Observations.Count + budgetIncrement)
                )).ToArray();
        }

        //The probability for the objective l of solution j to be less or equal than the objective l of solution i
        public static double ProbLE(StochasticSolution i, StochasticSolution j, int l)
        {
            var diff = i.Objectives[l] - j.Objectives[l];
            var standardError = Math.Sqrt(
                Math.Pow(i.StandardDeviations[l], 2) / i.Observations.Count +
                Math.Pow(j.StandardDeviations[l], 2) / j.Observations.Count);
            return Normal.CDF(0, standardError, diff);
        }
    }
}
