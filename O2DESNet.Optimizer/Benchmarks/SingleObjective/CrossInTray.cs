using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    //http://www.sfu.ca/~ssurjano/crossit.html
    public class CrossInTray : SingleObjective
    {
        public override double Evaluate(DenseVector x)
        {
            return -0.0001 * Math.Pow(Math.Abs(Math.Sin(x[0]) * Math.Sin(x[1]) * Math.Exp(Math.Abs(100 - x.L2Norm() / Math.PI))) + 1, 0.1);
        }

        protected override double DomainScale { get { return 10; } }
        protected override DenseVector[] GetOptimum(int dim)
        {
            return new DenseVector[] {
                new double[] { 1.3491, -1.3491 },
                new double[] { 1.3491, 1.3491 },
                new double[] { -1.3491, 1.3491 },
                new double[] { -1.3491, -1.3491 },
            };
        }
    }
}
