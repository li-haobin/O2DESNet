using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class Perm : SingleObjective
    {
        static double _beta = 0.5;
        public override double Evaluate(DenseVector x)
        {
            return Enumerable.Range(1, x.Count).Sum(i => Math.Pow(
                Enumerable.Range(1, x.Count).Sum(j => (Math.Pow(j, i) + _beta) * (Math.Pow(x[j - 1] / j, i) - 1))
                , 2));
        }
        public override double[] Start(int dim, Random rs)
        {
            return Enumerable.Range(0, dim).Select(i => rs.NextDouble() * 2 * dim - dim).ToArray();
        }
        protected override DenseVector[] GetOptimum(int dim)
        {
            return new DenseVector[] { Enumerable.Range(1, dim).Select(i => (double)i).ToArray() };
        }
    }
}
