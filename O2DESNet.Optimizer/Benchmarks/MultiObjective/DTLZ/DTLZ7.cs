using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public abstract class DTLZ7 : DTLZx
    {
        public DTLZ7(int nDecisions = 12, int nObjectives = 3)
        {
            _name = "DTLZ7";
            NDecisions = nDecisions;
            NObjectives = nObjectives;
        }

        public override DenseVector Evaluate(DenseVector decisions)
        {
            FeasibilityCheck(decisions);
            var x = decisions.ToArray();
            int k = NDecisions - NObjectives + 1;
            double[] f = new double[NObjectives];
            double[] theta = new double[NObjectives - 1];
            double g = 0.0;
            for (int i = NDecisions - k; i < NDecisions; i++)
                g += x[i];
            g = 1 + (9.0 * g) / k;
            for (int i = 0; i < NObjectives - 1; i++)
                f[i] = x[i];
            double h = 0.0;
            for (int i = 0; i < NObjectives - 1; i++)
                h += (f[i] / (1.0 + g)) * (1 + Math.Sin(3.0 * Math.PI * f[i]));
            h = NObjectives - h;
            f[NObjectives - 1] = (1 + g) * h;
            return f;
        }
    }
}
