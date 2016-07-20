using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class StyblinskiTang : SingleObjective
    {
        public override double Evaluate(DenseVector x) { return x.Sum(xi => Math.Pow(xi, 4) - Math.Pow(xi, 2) * 16 + xi * 5) / 2; }
        public override double[] Gradient(DenseVector x) { return x.Select(xi => 2.0 * xi * xi * xi - 16 * xi + 2.5).ToArray(); }
        protected override DenseVector[] GetOptimum(int dim)
        {
            return new DenseVector[] { Enumerable.Range(0, dim).Select(i => -2.903534).ToArray(), };
        }
    }
}
