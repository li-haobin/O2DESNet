using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public class Rosenbrock : SingleObjective
    {
        public override double Evaluate(DenseVector x)
        {
            double value = 0;
            for (int i = 0; i < x.Count - 1; i++) value += 100 * Math.Pow(x[i + 1] - x[i] * x[i], 2) + Math.Pow(x[i] - 1, 2);
            return value;
        }

        public override double[] Gradient(DenseVector x)
        {
            List<double> gradient = new List<double>();
            for (int i = 0; i < x.Count; i++)
            {
                if (i == 0) gradient.Add(-400 * (x[1] - x[0] * x[0]) * x[0] + 2 * x[0] - 2); // any problem?
                else if (i < x.Count - 1) gradient.Add(200 * (x[i] - x[i - 1] * x[i - 1]) - 400 * (x[i + 1] - x[i] * x[i]) * x[i] + 2 * x[i] - 2);
                else gradient.Add(200 * (x[i] - x[i - 1] * x[i - 1]));
            }
            return gradient.ToArray();
        }
        public override string ToString() { return "Rosenbrock"; }
    }
}
