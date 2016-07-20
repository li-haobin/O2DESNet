using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class ZDT4 : ZDTx
    {
        public ZDT4(int nDecisions) : base() { NDecisions = nDecisions; }
        public override string ToString() { return "ZDT4"; }
        public override DenseVector Evaluate(DenseVector decisions)
        {
            if (!FeasibilityCheck(decisions))
                return new double[] { double.PositiveInfinity, double.PositiveInfinity };
            int m = decisions.Count();
            double f1, f2, g, h;
            f1 = decisions.First();
            g = 1 + 10 * (m - 1);
            for (int i = 1; i < m; i++)
            {
                double xi = 10.0 * decisions.ElementAt(i) - 5.0;
                g += xi * xi - 10 * Math.Cos(4.0 * Math.PI * xi);
            }
            h = 1.0 - Math.Sqrt(f1 / g);
            f2 = g * h;
            return new double[] { f1, f2 };
        }
    }
}
