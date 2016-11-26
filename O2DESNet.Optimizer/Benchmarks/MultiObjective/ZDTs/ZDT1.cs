using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class ZDT1 : MultiObjective
    {
        public ZDT1(int nDecisions) : base() { NDecisions = nDecisions; NObjectives = 2; }
        protected bool FeasibilityCheck(DenseVector decisions) { return DecisionSpace.Contains(decisions); }
        public override ConvexSet DecisionSpace { get { return new ConvexSet(NDecisions, 0, 1); } }
        public override DenseVector Start(Random rs) { return Enumerable.Range(0, NDecisions).Select(i => rs.NextDouble()).ToArray(); }

        public override string ToString() { return "ZDT1"; }
        
        #region Function Values
        public override DenseVector Evaluate(DenseVector x)
        {
            if (!FeasibilityCheck(x))
                return new double[] { double.PositiveInfinity, double.PositiveInfinity };
            return new double[] { Vg1(x), Vg2(x) };
        }
        protected virtual double Vg1(DenseVector x) { return x.First(); }
        protected virtual double Vg2(DenseVector x) { return Vf(x) * Vh(x); }
        protected virtual double Vf(DenseVector x)
        {
            double f = 1.0;
            for (int i = 1; i < x.Count; i++) f += 9 * x[i] / (x.Count - 1);
            return f;
        }
        protected virtual double Vh(DenseVector x) { return 1 - Math.Sqrt(Vg1(x) / Vf(x)); }
        #endregion
        
        #region Derivatives
        public override DenseMatrix Gradients(DenseVector x) { return DenseMatrix.OfRowVectors(new DenseVector[] { Dg1(x), Dg2(x) }); }
        protected virtual DenseVector Dg1(DenseVector x) { var d = new double[x.Count]; d[0] = 1; return d; }
        protected virtual DenseVector Dg2(DenseVector x) { return Vf(x) * Dh(x) + Vh(x) * Df(x); }
        protected virtual DenseVector Df(DenseVector x) { var d = x.Select(v => 9.0 / (x.Count - 1)).ToArray(); d[0] = 0; return d; }
        protected virtual DenseVector Dh(DenseVector x)
        {
            return 0.5 * (Math.Sqrt(Vg1(x) / Math.Pow(Vf(x), 3)) * Df(x) - Math.Sqrt(1 / (Vg1(x) * Vf(x))) * Dg1(x));
        }
        #endregion
    }
}
