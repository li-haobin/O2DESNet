using O2DESNet.Explorers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Benchmarks
{
    public abstract class DTLZx : Benchmark
    {
        public DTLZx(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels)
        {
            NObjectives = noiseLevels.Length;
            // feasibility check
            bool feasible = Dimension > 0 && NObjectives > 0;
            foreach (double x in decisions) if (x < 0 || x > 1) { feasible = false; break; }
            if (!feasible) throw new Exception("Problem setting is infeasible.");
        }
        public static DecisionSpace DecisionSpace(int dim)
        {
            return new DecisionSpace(
                Enumerable.Range(0, dim).Select(i => 0.0).ToArray(),
                Enumerable.Range(0, dim).Select(i => 1.0).ToArray());
        }
    }

    public class DTLZ1 : DTLZx
    {
        public DTLZ1(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            var x = Decisions.ToArray();
            int k = Dimension - NObjectives + 1;
            double[] f = new double[NObjectives];
            double g = 0.0;
            for (int i = Dimension - k; i < Dimension; i++)
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

    public class DTLZ2 : DTLZx
    {
        public DTLZ2(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            var x = Decisions.ToArray();
            int k = Dimension - NObjectives + 1;
            double[] f = new double[NObjectives];
            double g = 0.0;
            for (int i = Dimension - k; i < Dimension; i++)
                g += (x[i] - 0.5) * (x[i] - 0.5);
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

    public class DTLZ3 : DTLZx
    {
        public DTLZ3(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            var x = Decisions.ToArray();
            int k = Dimension - NObjectives + 1;
            double[] f = new double[NObjectives];
            double g = 0.0;
            for (int i = Dimension - k; i < Dimension; i++)
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

    public class DTLZ4 : DTLZx
    {
        public DTLZ4(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            var x = Decisions.ToArray();
            int k = Dimension - NObjectives + 1;
            double[] f = new double[NObjectives];
            double alpha = 100.0;
            double g = 0.0;
            for (int i = Dimension - k; i < Dimension; i++)
                g += (x[i] - 0.5) * (x[i] - 0.5);
            for (int i = 0; i < NObjectives; i++)
                f[i] = 1.0 + g;
            for (int i = 0; i < NObjectives; i++)
            {
                for (int j = 0; j < NObjectives - (i + 1); j++)
                {
                    f[i] *= Math.Cos(Math.Pow(x[j], alpha) * Math.PI / 2.0);
                }
                if (i != 0)
                {
                    int aux = NObjectives - (i + 1);
                    f[i] *= Math.Sin(Math.Pow(x[aux], alpha) * Math.PI / 2.0);
                }
            }
            return f;
        }
    }

    public class DTLZ5 : DTLZx
    {
        public DTLZ5(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            var x = Decisions.ToArray();
            int k = Dimension - NObjectives + 1;
            double[] f = new double[NObjectives];
            double[] theta = new double[NObjectives - 1];
            double g = 0.0;
            for (int i = Dimension - k; i < Dimension; i++)
                g += (x[i] - 0.5) * (x[i] - 0.5);
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

    public class DTLZ6 : DTLZx
    {
        public DTLZ6(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            var x = Decisions.ToArray();
            int k = Dimension - NObjectives + 1;
            double[] f = new double[NObjectives];
            double[] theta = new double[NObjectives - 1];
            double g = 0.0;
            for (int i = Dimension - k; i < Dimension; i++)
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

    public class DTLZ7 : DTLZx
    {
        public DTLZ7(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            var x = Decisions.ToArray();
            int k = Dimension - NObjectives + 1;
            double[] f = new double[NObjectives];
            double[] theta = new double[NObjectives - 1];
            double g = 0.0;
            for (int i = Dimension - k; i < Dimension; i++)
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
