using System;
using System.Linq;

namespace O2DESNet.Benchmarks
{
    public abstract class ZDTx : Benchmark
    {
        public ZDTx(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels)
        {
            NObjectives = 2;
            // feasibility check
            bool feasible = Dimension > 0 && noiseLevels.Length == NObjectives;
            foreach (double x in decisions) if (x < 0 || x > 1) { feasible = false; break; }
            if (!feasible) throw new Exception("Problem setting is infeasible.");
        }
        protected int m { get { return Dimension; } }
    }

    public class ZDT1 : ZDTx
    {
        public ZDT1(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            double f1, f2, g, h;
            f1 = Decisions.First();
            g = 1.0; for (int i = 1; i < m; i++) g += 9 * Decisions[i] / (m - 1);
            h = 1 - Math.Sqrt(f1 / g);
            f2 = g * h;
            return new double[] { f1, f2 };
        }
    }

    public class ZDT2 : ZDTx
    {
        public ZDT2(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            double f1, f2, g, h;
            f1 = Decisions.First();
            g = 1; for (int i = 1; i < m; i++) g += 9 * Decisions[i] / (m - 1);
            h = 1 - Math.Pow(f1 / g, 2.0);
            f2 = g * h;
            return new double[] { f1, f2 };
        }
    }

    public class ZDT3 : ZDTx
    {
        public ZDT3(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            double f1, f2, g, h;
            f1 = Decisions.First();
            g = 1;
            for (int i = 1; i < m; i++) g += 9 * Decisions[i] / (m - 1);
            h = 1 - Math.Sqrt(f1 / g) - f1 / g * Math.Sin(10.0 * Math.PI * f1);
            f2 = g * h;
            return new double[] { f1, f2 };
        }
    }

    public class ZDT4 : ZDTx
    {
        public ZDT4(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            double f1, f2, g, h;
            f1 = Decisions.First();
            g = 1 + 10 * (m - 1);
            for (int i = 1; i < m; i++)
            {
                double xi = 10.0 * Decisions[i] - 5.0;
                g += xi * xi - 10 * Math.Cos(4.0 * Math.PI * xi);
            }
            h = 1.0 - Math.Sqrt(f1 / g);
            f2 = g * h;
            return new double[] { f1, f2 };
        }
    }

    public class ZDT6 : ZDTx
    {
        public ZDT6(double[] decisions, double[] noiseLevels) : base(decisions, noiseLevels) { }
        public override double[] CalObjectives()
        {
            double x1, f1, f2, g, h;
            x1 = Decisions.First();
            f1 = 1 - Math.Exp(-4.0 * x1) * Math.Pow(Math.Sin(6.0 * Math.PI * x1), 6);
            g = 0; for (int i = 1; i < m; i++) g += Decisions[i];
            g = 1 + 9 * Math.Pow(g / (m - 1), 0.25);
            h = 1 - Math.Pow(f1 / g, 2);
            f2 = g * h;
            return new double[] { f1, f2 };
        }
    }
}