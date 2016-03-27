using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Optimizers
{
    public class DecisionSpace
    {
        public int Dimension { get; private set; }
        public decimal[] Lowerbounds { get; private set; }
        public decimal[] Upperbounds { get; private set; }
        public List<List<decimal>> Constraints { get; private set; }

        public DecisionSpace(IEnumerable<decimal> lbs, IEnumerable<decimal> ubs)
        {
            Dimension = lbs.Count();
            if (Dimension != ubs.Count()) throw new InconsistentDimension();
            Lowerbounds = lbs.ToArray();
            Upperbounds = ubs.ToArray();
            Constraints = new List<List<decimal>>();
        }

        public void AddCstrLe(IEnumerable<decimal> coeffs, decimal bound)
        {
            if (coeffs.Count() != Dimension) throw new InconsistentDimension();
            Constraints.Add(coeffs.Concat(new decimal[] { bound }).ToList());
        }
        public void AddCstrGe(IEnumerable<decimal> coeffs, decimal bound)
        {
            AddCstrLe(coeffs.Select(c => -c), -bound);
        }
        internal bool IsFeasible(decimal[] decision)
        {
            if (decision.Length != Dimension) throw new InconsistentDimension();
            foreach (var cstr in Constraints)
                if (Enumerable.Range(0, Dimension).Sum(i => decision[i] * cstr[i]) > cstr.Last()) return false;
            return true;
        }
        /// <summary>
        /// Random sample decisions within the space
        /// </summary>
        public List<decimal[]> Sample(int size, Random rs)
        {
            var decisions = new List<decimal[]>();
            while (decisions.Count < size)
            {
                var decision = Enumerable.Range(0, Dimension).Select(i => Lowerbounds[i] + (Upperbounds[i] - Lowerbounds[i]) * Convert.ToDecimal(rs.NextDouble())).ToArray();
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
