using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public abstract class DTLZx : MultiObjective
    {
        protected string _name;        
        protected void FeasibilityCheck(DenseVector decisions)
        {
            bool feasible = decisions.Count() > 0;
            foreach (double x in decisions) if (x < 0 || x > 1) { feasible = false; break; }
            if (!feasible) throw new Exception("Decisions are Infeasible.");
        }
        public override string ToString() { return string.Format("{0} ({1}-{2})", _name, NDecisions, NObjectives); }
        public override ConvexSet DecisionSpace { get { return new ConvexSet(NDecisions, 0, 1); } }
        public override DenseVector Start(Random rs) { return Enumerable.Range(0, NDecisions).Select(i => rs.NextDouble()).ToArray(); }
    }
}
