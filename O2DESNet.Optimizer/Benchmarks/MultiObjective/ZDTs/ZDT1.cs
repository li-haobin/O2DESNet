using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class ZDT1 : ZDTx
    {
        public ZDT1(int nDecisions) : base() { NDecisions = nDecisions; }
        public override string ToString() { return "ZDT1"; }
        public override DenseVector Evaluate(DenseVector decisions)
        {
            if (!FeasibilityCheck(decisions))
                return new double[] { double.PositiveInfinity, double.PositiveInfinity };
            int m = decisions.Count();
            double f1, f2, g, h;
            f1 = decisions.First();
            g = 1.0; for (int i = 1; i < m; i++) g += 9 * decisions.ElementAt(i) / (m - 1);
            h = 1 - Math.Sqrt(f1 / g);
            f2 = g * h;
            return new double[] { f1, f2 };
        }
    }
}
