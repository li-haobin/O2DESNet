using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class GoldsteinPrice : SingleObjective
    {
        private static double s1(double x1, double x2) { return (x1 + x2 + 1) * (x1 + x2 + 1); }
        private static double s2(double x1, double x2) { return 19 - 14 * x1 + 3 * x1 * x1 - 14 * x2 + 6 * x1 * x2 + 3 * x2 * x2; }
        private static double s3(double x1, double x2) { return (2 * x1 - 3 * x2) * (2 * x1 - 3 * x2); }
        private static double s4(double x1, double x2) { return 18 - 32 * x1 + 12 * x1 * x1 + 48 * x2 - 36 * x1 * x2 + 27 * x2 * x2; }
        private static double t1(double x1, double x2) { return 1 + s1(x1, x2) * s2(x1, x2); }
        private static double t2(double x1, double x2) { return 30 + s3(x1, x2) * s4(x1, x2); }

        private static double ds1dx1(double x1, double x2) { return 2 * (x1 + x2 + 1); }
        private static double ds1dx2(double x1, double x2) { return 2 * (x1 + x2 + 1); }
        private static double ds2dx1(double x1, double x2) { return -14 + 6 * x1 + 6 * x2; }
        private static double ds2dx2(double x1, double x2) { return -14 + 6 * x1 + 6 * x2; }
        private static double ds3dx1(double x1, double x2) { return 4 * (2 * x1 - 3 * x2); }
        private static double ds3dx2(double x1, double x2) { return -6 * (2 * x1 - 3 * x2); }
        private static double ds4dx1(double x1, double x2) { return -32 + 24 * x1 - 36 * x2; }
        private static double ds4dx2(double x1, double x2) { return 48 - 36 * x1 + 54 * x2; }
        private static double dt1dx1(double x1, double x2) { return s1(x1, x2) * ds2dx1(x1, x2) + ds1dx1(x1, x2) * s2(x1, x2); }
        private static double dt1dx2(double x1, double x2) { return s1(x1, x2) * ds2dx2(x1, x2) + ds1dx2(x1, x2) * s2(x1, x2); }
        private static double dt2dx1(double x1, double x2) { return s3(x1, x2) * ds4dx1(x1, x2) + ds3dx1(x1, x2) * s4(x1, x2); }
        private static double dt2dx2(double x1, double x2) { return s3(x1, x2) * ds4dx2(x1, x2) + ds3dx2(x1, x2) * s4(x1, x2); }

        public override double Evaluate(DenseVector x)
        {
            double x1 = x[0], x2 = x[1];
            return t1(x1, x2) * t2(x1, x2);
        }

        public override double[] Gradient(DenseVector x)
        {
            double x1 = x[0], x2 = x[1];
            return new double[] {
                t1(x1, x2) * dt2dx1(x1, x2) + dt1dx1(x1, x2) * t2(x1, x2),
                t1(x1, x2) * dt2dx2(x1, x2) + dt1dx2(x1, x2) * t2(x1, x2)
            };
        }

        protected override double DomainScale { get { return 2; } }

        protected override DenseVector[] GetOptimum(int dim)
        {
            return new DenseVector[] { new double[] { 0, -1 }, };
        }

        public override string ToString() { return "GoldsteinPrice"; }
    }
}
