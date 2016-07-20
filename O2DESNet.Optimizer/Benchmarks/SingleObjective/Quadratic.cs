using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class Quadratic : SingleObjective
    {
        public override double Evaluate(DenseVector x) { return x.Sum(v => v * v); }

        public override double[] Gradient(DenseVector x) { return x.Select(v => v * 2).ToArray(); }
    }
}
