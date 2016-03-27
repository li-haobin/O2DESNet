using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Optimizers
{
    public class DecisionSpace
    {
        public int Dimension { get; private set; }
        public double[] Lowerbounds { get; private set; }
        public double[] Upperbounds { get; private set; }
        public List<List<double>> Constraints { get; private set; }

        public DecisionSpace(IEnumerable<double> lbs, IEnumerable<double> ubs)
        {
            Dimension = lbs.Count();
            if (Dimension != ubs.Count()) throw new InconsistentDimension();
            Lowerbounds = lbs.ToArray();
            Upperbounds = ubs.ToArray();
            Constraints = new List<List<double>>();
        }

        public void AddCstrLe(IEnumerable<double> coeffs, double bound)
        {
            if (coeffs.Count() != Dimension) throw new InconsistentDimension();
            Constraints.Add(coeffs.Concat(new double[] { bound }).ToList());
        }
        public void AddCstrGe(IEnumerable<double> coeffs, double bound)
        {
            AddCstrLe(coeffs.Select(c => -c), -bound);
        }
        internal bool IsFeasible(double[] decision)
        {
            if (decision.Length != Dimension) throw new InconsistentDimension();
            foreach (var cstr in Constraints)
                if (Enumerable.Range(0, Dimension).Sum(i => decision[i] * cstr[i]) > cstr.Last()) return false;
            return true;
        }
        /// <summary>
        /// Random sample decisions within the space
        /// </summary>
        public List<double[]> Sample(int size, Random rs)
        {
            var decisions = new List<double[]>();
            while (decisions.Count < size)
            {
                var decision = Enumerable.Range(0, Dimension).Select(i => Lowerbounds[i] + (Upperbounds[i] - Lowerbounds[i]) * rs.NextDouble()).ToArray();
                if (IsFeasible(decision)) decisions.Add(decision);
            }
            return decisions;
        }

        public class InconsistentDimension : Exception
        {
            public InconsistentDimension() : base() { }
            public InconsistentDimension(string message) : base(message) { }
        }
    }
}
