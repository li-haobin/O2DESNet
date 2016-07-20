using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class PoguSumExp : SingleObjective
    {
        public override double Evaluate(DenseVector x)
        {
            double value = 0;
            for (int j = 1; j <= 41; j++) value += Math.Pow(inner(x, j), 2);
            return value / 41;
        }

        public override double[] Gradient(DenseVector x)
        {
            List<double> gradient = new List<double>();
            for (int i = 1; i <= NEXP * 2; i++)
            {
                double gi = 0;
                for (int j = 1; j <= 41; j++)
                {
                    double inn = inner(x, j);
                    double innerP = innerPartialDerivative(x, i, j);

                    gi += 2 * inn * innerP;
                    //gi = Math.Min(double.MaxValue, gi); 
                    //gi = Math.Max(double.MinValue, gi);
                }

                gradient.Add(gi / 41);
            }
            if (gradient.Count < x.Count) gradient.Add(0);
            return gradient.ToArray();
        }
        
        public int NEXP { get; private set; }
        public int Dimension { get; private set; }
        private void Init(DenseVector x) { Dimension = x.Count; NEXP = Dimension / 2; }
        

        private double t(int j) { return 0.025 * (j - 1); }
        private double y(int j) { return 1 + Math.Log(1 + t(j)); }
        private double inner(DenseVector x, int j)
        {
            double inner = y(j);
            double tj = t(j);
            for (int i = 1; i <= NEXP; i++) inner -= x[2 * i - 2] * Math.Exp(x[2 * i - 1] * tj);
            return inner;
        }

        private double innerPartialDerivative(DenseVector x, int i, int j)
        {
            if (i % 2 == 0) return -x[i - 2] * t(j) * Math.Exp(x[i - 1] * t(j));
            else return -Math.Exp(x[i] * t(j));
        }        
    }
}
