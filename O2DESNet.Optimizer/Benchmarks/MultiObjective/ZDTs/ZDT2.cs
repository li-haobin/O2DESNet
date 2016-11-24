using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class ZDT2 : ZDT1
    {
        public ZDT2(int nDecisions) : base(nDecisions) { }
        public override string ToString() { return "ZDT2"; }
        protected override double Vh(DenseVector x)
        {
            return 1 - Math.Pow(Vg1(x) / Vf(x), 2);
        }
        protected override DenseVector Dh(DenseVector x)
        {
            return 2 * (Math.Pow(Vg1(x), 2) / Math.Pow(Vf(x), 3) * Df(x) - Vg1(x) / Math.Pow(Vf(x), 2) * Dg1(x));
        }
    }
}
