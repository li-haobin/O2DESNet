using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class SixHumpCamel : SingleObjective
    {
        public override double Evaluate(DenseVector x)
        {
            double x1 = x[0], x2 = x[1];
            return (4 - 2.1 * x1 * x1 + x1 * x1 * x1 * x1 / 3) * x1 * x1 + x1 * x2 + (-4 + 4 * x2 * x2) * x2 * x2;
        }
        public override double[] Gradient(DenseVector x)
        {
            double x1 = x[0], x2 = x[1];
            return new double[] {
                8 * x1 - 8.4 * x1 * x1 * x1 + 2 * x1 * x1 * x1 * x1 * x1 + x2,
                x1 - 8 * x2 + 16 * x2 * x2 * x2
            };
        }
        public override double[] Start(int dim, Random rs)
        {
            return new double[] {
                (rs.NextDouble() * 2 - 1) * 3,
                (rs.NextDouble() * 2 - 1) * 2
            };
        }
        protected override DenseVector[] GetOptimum(int dim)
        {
            return new DenseVector[] {
                new double[] { 0.0898, -0.7126 },
                new double[] { -0.0898, 0.7126 }
            };
        }
    }
}
