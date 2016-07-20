using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public abstract class SingleObjective
    {
        public abstract double Evaluate(DenseVector x);
        public virtual double[] Gradient(DenseVector x)
        {
            return Gradient_FDSA(x, 1E-4);
        }
        public double DistanceToOptimum(DenseVector x)
        {
            var optimum = GetOptimum(x.Count);
            if (optimum.Length < 1) return double.PositiveInfinity;
            return optimum.Select(o => (x - o).L2Norm()).Min();
        }
        public double[] Gradient_FDSA(DenseVector x, double perturbation)
        {
            var g = new double[x.Count];
            if (perturbation <= 0) throw new Exception("The perturbation only takes positive value.");
            Parallel.For(0, x.Count, i => {
                var x1 = x.ToArray(); x1[i] += perturbation;
                var x2 = x.ToArray(); x2[i] -= perturbation;
                g[i] = (Evaluate(x2) - Evaluate(x1)) / perturbation / 2;
            });
            return g;
        }
        protected virtual double DomainScale { get { return 5; } }
        /// <summary>
        /// Sample a starting point given dimension and random stream
        /// </summary>
        public virtual double[] Start(int dim, Random rs)
        {
            return Enumerable.Range(0, dim).Select(i => (rs.NextDouble() * 2 - 1) * DomainScale).ToArray();
        }
        protected virtual DenseVector[] GetOptimum(int dim) { return new DenseVector[0]; }
    }    
}
