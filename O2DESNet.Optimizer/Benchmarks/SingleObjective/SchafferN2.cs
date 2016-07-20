using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class SchafferN2 : SingleObjective
    {
        public override double Evaluate(DenseVector x)
        {
            return 0.5 + (Math.Pow(Math.Sin(x[0] * x[0] - x[1] * x[1]), 2) - 0.5) / Math.Pow(1 + 0.001 * (x[0] * x[0] + x[1] * x[1]), 2);
        }
        protected override double DomainScale { get { return 100; } }
        protected override DenseVector[] GetOptimum(int dim)
        {
            return new DenseVector[] { new double[] { 0, 0 }, };
        }
    }
}
