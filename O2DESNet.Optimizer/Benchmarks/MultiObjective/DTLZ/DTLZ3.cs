using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public abstract class DTLZ3 : DTLZx
    {
        public DTLZ3(int nDecisions = 12, int nObjectives = 3)
        {
            _name = "DTLZ3";
            NDecisions = nDecisions;
            NObjectives = nObjectives;
        }

        public override DenseVector Evaluate(DenseVector decisions)
        {
            FeasibilityCheck(decisions);
            var x = decisions.ToArray();
            int k = NDecisions - NObjectives + 1;
            double[] f = new double[NObjectives];
            double g = 0.0;
            for (int i = NDecisions - k; i < NDecisions; i++)
                g += (x[i] - 0.5) * (x[i] - 0.5) - Math.Cos(20.0 * Math.PI * (x[i] - 0.5));
            g = 100.0 * (k + g);
            for (int i = 0; i < NObjectives; i++)
                f[i] = 1.0 + g;
            for (int i = 0; i < NObjectives; i++)
            {
                for (int j = 0; j < NObjectives - (i + 1); j++)
                {
                    f[i] *= Math.Cos(x[j] * 0.5 * Math.PI);
                }
                if (i != 0)
                {
                    int aux = NObjectives - (i + 1);
                    f[i] *= Math.Sin(x[aux] * 0.5 * Math.PI);
                }
            }
            return f;
        }
    }
}
