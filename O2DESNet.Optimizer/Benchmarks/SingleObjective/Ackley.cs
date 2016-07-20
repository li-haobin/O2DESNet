using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    //http://www.sfu.ca/~ssurjano/ackley.html
    public class Ackley : SingleObjective
    {
        private static double a = 20, b = 0.2, c = Math.PI * 2;
        public override double Evaluate(DenseVector x)
        {
            return -a * Math.Exp(-b * Math.Sqrt(x.Average(xi => xi * xi))) - Math.Exp(x.Average(xi => Math.Cos(c * xi))) + a + Math.Exp(1);
        }

        protected override double DomainScale
        {
            get
            {
                return 10;
                //return 32.768;
            }
        }
    }
}
