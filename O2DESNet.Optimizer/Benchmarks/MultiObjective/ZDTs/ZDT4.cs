using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class ZDT4 : ZDT1
    {
        public ZDT4(int nDecisions) : base(nDecisions) { }
        public override string ToString() { return "ZDT4"; }
        protected override double Vf(DenseVector x)
        {
            var xp = 10 * x - 5;
            return 1 + 10 * (NDecisions - 1) + Enumerable.Range(1, NDecisions - 1).Sum(i => xp[i] * xp[i] - 10 * Math.Cos(4 * Math.PI * xp[i]));
        }
        protected override DenseVector Df(DenseVector x)
        {
            var xp = 10 * x - 5;
            return new List<double> { 0 }.Concat(Enumerable.Range(1, NDecisions - 1).Select(i => 20 * xp[i] + 400 * Math.PI * Math.Sin(4 * Math.PI * xp[i]))).ToArray();
        }
    }
}
