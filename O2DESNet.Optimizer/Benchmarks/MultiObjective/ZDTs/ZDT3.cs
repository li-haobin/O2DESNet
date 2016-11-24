using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class ZDT3 : ZDT1
    {
        public ZDT3(int nDecisions) : base(nDecisions) { }
        public override string ToString() { return "ZDT3"; }
        protected override double Vh(DenseVector x)
        {
            return base.Vh(x) - Vg1(x) / Vf(x) * Math.Sin(Kernel(x));
        }
        protected override DenseVector Dh(DenseVector x)
        {
            double kernel = Kernel(x), f = Vf(x), g1 = Vg1(x);
            return base.Dh(x) 
                - (Math.Sin(kernel) + kernel * Math.Cos(kernel)) / f * Dg1(x) 
                + g1 * Math.Sin(kernel) / (f * f) * Df(x);
        }
        private double Kernel(DenseVector x) { return 10 * Math.PI * Vg1(x); }
    }
}
