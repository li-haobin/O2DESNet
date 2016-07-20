using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    // http://www.sfu.ca/~ssurjano/bukin6.html
    public class BukinN6 : SingleObjective
    {
        public override double Evaluate(DenseVector x)
        {
            return Math.Sqrt(Math.Abs(x[1] - 0.01 * x[0] * x[0])) * 100 + 0.01 * Math.Abs(x[0] + 10);
        }

        public override double[] Start(int dim, Random rs)
        {
            return new double[] { rs.NextDouble() * 10 - 15, rs.NextDouble() * 6 - 3 };
        }
    }
}
