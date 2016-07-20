using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class ZDT6 : ZDTx
    {
        public ZDT6(int nDecisions) : base() { NDecisions = nDecisions; }
        public override string ToString() { return "ZDT6"; }
        public override DenseVector Evaluate(DenseVector decisions)
        {
            if (!FeasibilityCheck(decisions))
                return new double[] { double.PositiveInfinity, double.PositiveInfinity };
            int m = decisions.Count();
            double x1, f1, f2, g, h;
            x1 = decisions.First();
            f1 = 1 - Math.Exp(-4.0 * x1) * Math.Pow(Math.Sin(6.0 * Math.PI * x1), 6);
            g = 0; for (int i = 1; i < m; i++) g += decisions.ElementAt(i);
            g = 1 + 9 * Math.Pow(g / (m - 1), 0.25);
            h = 1 - Math.Pow(f1 / g, 2);
            f2 = g * h;
            return new double[] { f1, f2 };
        }
    }
}
