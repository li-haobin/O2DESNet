using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class MultiSphere : MultiObjective
    {
        public DenseVector[] Optima { get; private set; }
        public MultiSphere(DenseVector[] optima = null)
        {
            if (optima == null) optima = new DenseVector[] {
                new double[] { 0, 0  },
                new double[] { 3, 3 },
            };
            NObjectives = optima.Length;
            NDecisions = optima[0].Count;
            Optima = optima;
        }

        public override ConvexSet DecisionSpace { get { return new ConvexSet(NDecisions, -10, 10); } }
        public override DenseVector Evaluate(DenseVector x) { return Optima.Select(o => (x - o).Sum(v => v * v)).ToArray(); }
        public override DenseVector Start(Random rs) { return Enumerable.Range(0, NDecisions).Select(i => rs.NextDouble() * 20 - 10).ToArray(); }
    }
}
