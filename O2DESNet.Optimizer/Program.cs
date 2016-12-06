using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using O2DESNet.Optimizer.Samplings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Test_ForOCBAmcvr();
        }

        static void Test_ForOCBAmcvr()
        {
            var ocba1 = new OCBA();
            var ocba2 = new OCBAmcvr();

            var rns1 = new Benchmarks.RnS_MonotoneDecreasingMeans(nDesigns: 10, vPower: 1);
            var rns2 = new Benchmarks.RnS_MonotoneDecreasingMeans(nDesigns: 10, vPower: 1);

            for (int i = 0; i < 100; i++)
            {
                var alloc1 = ocba1.Alloc(10, rns1.Solutions);
                foreach (var a in alloc1) rns1.Evaluate((int)a.Key[0], a.Value);

                var alloc2 = ocba2.Alloc(10, rns2.Solutions);
                foreach (var a in alloc2) rns2.Evaluate((int)a.Key[0], a.Value);

                Console.WriteLine("{0:F4}\t{1:F4}\t{2:F4}\t{3:F4}", rns1.PCS, rns2.PCS, rns1.Variance, rns2.Variance);
                Console.ReadKey();
            }          
        }

        static void Test_ForOCBA()
        {
            var candidates = new List<StochasticSolution> {
                new StochasticSolution(new double[] { 1 }, new DenseVector[] { new double[] { 1 }, new double[] { 2 } }),
                //new StochasticSolution(new double[] { 1 }, new DenseVector[] { new double[] { 0 }, new double[] { 3 } }),
                new StochasticSolution(new double[] { 2 }, new DenseVector[] { new double[] { 0 }, new double[] { 3 } }),
                new StochasticSolution(new double[] { 3 }, new DenseVector[] { new double[] { 2 }, new double[] { 3.1 } }),
                new StochasticSolution(new double[] { 4 }, new DenseVector[] { new double[] { 2 }, new double[] { 4.1 } }),
            };

            //var allocation = new OCBA().Alloc(100, candidates);
            var allocation = new MOCBA().Alloc(1000, candidates);
        }

        static void Test_ForMoCompass()
        {
            var moCompass = new MoCompass(new ConvexSet(3,
               globalLb: 5,
               constraints: new Constraint[] {
                    new ConstraintGE(new double[] { 1, 1, 1}, 12)
               }));

            Func<DenseVector, double> f1 = p => (p - new double[] { 1, 2, 3 }).L2Norm();
            Func<DenseVector, double> f2 = p => (p - new double[] { 10, 11, 12 }).L2Norm();

            var rs = new Random(0);
            while (true)
            {
                var points = moCompass.Sample(10, 0);
                moCompass.Enter(points.Select(p => new StochasticSolution(p, new double[] { f1(p), f2(p) })));
                if (points.Length < 1) break;
                Console.Clear();
                //foreach (var p in points) Console.WriteLine("{0:F4}\t{1:F4}\t{2:F4}->\t{3:F4}\t{4:F4}", p[0], p[1], p[2], f1(p), f2(p));
                foreach (var p in moCompass.ParetoSet.OrderBy(p => p.Objectives[0])) Console.WriteLine("{0:F4},{1:F4}", p.Objectives[0], p.Objectives[1]);
                Console.ReadKey();
            }
            var samples = moCompass.Sample(10);
        }
    }
}
