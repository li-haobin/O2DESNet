using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public abstract class DTLZ1 : DTLZx
    {
        public DTLZ1(int nDecisions = 12, int nObjectives = 3)
        {
            _name = "DTLZ1";
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
            g = 100 * (k + g);
            for (int i = 0; i < NObjectives; i++)
                f[i] = (1.0 + g) * 0.5;
            for (int i = 0; i < NObjectives; i++)
            {
                for (int j = 0; j < NObjectives - (i + 1); j++)
                {
                    f[i] *= x[j];
                }
                if (i != 0)
                {
                    int aux = NObjectives - (i + 1);
                    f[i] *= 1 - x[aux];
                }
            }
            return f;
        }
    }
}
