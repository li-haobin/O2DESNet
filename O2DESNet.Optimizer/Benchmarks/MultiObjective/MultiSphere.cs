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
                new double[] { 0.5, 0.5  },
                new double[] { 0.8, 0.8 },
            };
            NObjectives = optima.Length;
            NDecisions = optima[0].Count;
            Optima = optima;
        }

        public override ConvexSet DecisionSpace { get { return new ConvexSet(NDecisions, 0, 1); } }
        public override DenseVector Evaluate(DenseVector x) { return Optima.Select(o => (x - o).Sum(v => v * v)).ToArray(); }
        public override DenseVector Start(Random rs) { return Enumerable.Range(0, NDecisions).Select(i => rs.NextDouble()).ToArray(); }

        public override DenseMatrix Gradients(DenseVector x)
        {
            return DenseMatrix.OfRowVectors(Optima.Select(o => 2 * (x - o)));
        }
    }
}
