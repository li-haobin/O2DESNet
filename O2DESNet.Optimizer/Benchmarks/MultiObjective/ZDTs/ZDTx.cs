using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public abstract class ZDTx : MultiObjective
    {
        protected ZDTx() { NObjectives = 2; }
        protected bool FeasibilityCheck(DenseVector decisions) { return DecisionSpace.Contains(decisions); }        
        public override ConvexSet DecisionSpace { get { return new ConvexSet(NDecisions, 0, 1); } }
        public override DenseVector Start(Random rs) { return Enumerable.Range(0, NDecisions).Select(i => rs.NextDouble()).ToArray(); }
    }
}
