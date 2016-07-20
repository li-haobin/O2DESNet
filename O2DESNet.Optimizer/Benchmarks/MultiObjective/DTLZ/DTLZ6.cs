using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public abstract class DTLZ6 : DTLZx
    {
        public DTLZ6(int nDecisions = 12, int nObjectives = 3)
        {
            _name = "DTLZ6";
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
                g += Math.Pow(x[i], 0.1);
            double t = Math.PI / (4.0 * (1.0 + g));
            theta[0] = x[0] * Math.PI / 2.0;
            for (int i = 1; i < NObjectives - 1; i++)
                theta[i] = t * (1.0 + 2.0 * g * x[i]);
            for (int i = 0; i < NObjectives; i++)
                f[i] = 1.0 + g;
            for (int i = 0; i < NObjectives; i++)
            {
                for (int j = 0; j < NObjectives - (i + 1); j++)
                    f[i] *= Math.Cos(theta[j]);
                if (i != 0)
                {
                    int aux = NObjectives - (i + 1);
                    f[i] *= Math.Sin(theta[aux]);
                }
            }
            return f;
        }
    }
}
