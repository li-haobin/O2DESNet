using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public abstract class MultiObjective
    {
        public int NDecisions { get; protected set; }
        public int NObjectives { get; protected set; }
        public abstract DenseVector Evaluate(DenseVector x);
        public virtual DenseMatrix Gradients(DenseVector x)
        {
            return Gradients_FDSA(x, 1E-12);
        }
        public DenseMatrix Gradients_FDSA(DenseVector x, double perturbation)
        {
            var g = new List<DenseVector>();
            if (perturbation <= 0) throw new Exception("The perturbation only takes positive value.");
            for (int i = 0; i < x.Count; i++)
            {
                var x1 = x.ToArray(); x1[i] += perturbation;
                var x2 = x.ToArray(); x2[i] -= perturbation;
                if (DecisionSpace.Contains(x1) && DecisionSpace.Contains(x2)) g.Add((Evaluate(x1) - Evaluate(x2)) / perturbation / 2);
                else if (DecisionSpace.Contains(x1)) g.Add((Evaluate(x1) - Evaluate(x)) / perturbation);
                else g.Add((Evaluate(x) - Evaluate(x2)) / perturbation);
            }
            return DenseMatrix.OfColumnVectors(g);
        }

        public abstract ConvexSet DecisionSpace { get; }
        public abstract DenseVector Start(Random rs);
    }
}
