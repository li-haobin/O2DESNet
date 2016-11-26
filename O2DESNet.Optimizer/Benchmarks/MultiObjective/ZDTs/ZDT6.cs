using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class ZDT6 : ZDT2
    {
        public ZDT6(int nDecisions) : base(nDecisions) { }
        public override string ToString() { return "ZDT6"; }
        protected override double Vg1(DenseVector x)
        {
            return 1 - Math.Exp(-4 * x[0]) * Math.Pow(Math.Sin(6 * Math.PI * x[0]), 6);
        }
        protected override DenseVector Dg1(DenseVector x)
        {
            var kernel = 6 * Math.PI * x[0];
            var d = new double[NDecisions];
            d[0] = (-36 * Math.PI * Math.Pow(Math.Sin(kernel), 5) * Math.Cos(kernel) + 4 * Math.Pow(Math.Sin(kernel), 6)) * Math.Exp(-4 * x[0]);
            return d;
        }
        protected override double Vf(DenseVector x)
        {
            return 1 + 9 * Math.Pow(Enumerable.Range(1, NDecisions - 1).Sum(i => x[i]) / (NDecisions - 1), 0.25);
        }
        protected override DenseVector Df(DenseVector x)
        {
            return new double[] { 0 }.Concat(Enumerable.Range(1, NDecisions - 1).Select(i => 9 * 0.25 / (NDecisions - 1) * Math.Pow(Enumerable.Range(1, NDecisions - 1).Sum(j => x[j]) / (NDecisions - 1), -0.75))).ToArray();
        }
    }
}
