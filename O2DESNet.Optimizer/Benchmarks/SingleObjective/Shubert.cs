using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class Shubert : SingleObjective
    {
        public override double Evaluate(DenseVector x)
        {
            var indices = Enumerable.Range(0, 5);
            return indices.Sum(i => Math.Cos(x[0] * (i + 1) + i) * i) * indices.Sum(i => Math.Cos(x[1] * (i + 1) + i) * i);
        }

        protected override double DomainScale { get { return 10; } }
    }
}
