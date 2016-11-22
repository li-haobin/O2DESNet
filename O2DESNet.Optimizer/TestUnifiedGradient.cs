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
    class TestUnifiedGradient
    {
        static void Main(string[] args)
        {
            int dimension = 3;
            int paretoSize = 10;

            var rs = new Random(0);
            var paretoSet = new List<DenseVector>();
            while (paretoSet.Count < paretoSize)
            {
                paretoSet.Add(Enumerable.Range(0, dimension).Select(i => rs.NextDouble()).ToArray());
                paretoSet = Pareto.GetParetoSet(paretoSet).ToList();
            }

            var weights = Pareto.UnifiedWeightingVectors(paretoSet, Enumerable.Range(0, dimension).Select(i => 1.0).ToArray());
            foreach (var p in paretoSet.OrderBy(p => p[0]).ThenBy(p => p[1]))
            {
                Console.Write("{0},{1},{2},,", p[0], p[1], p[2]);
                var w = weights[p];
                Console.WriteLine("{0},{1},{2}", w[0], w[1], w[2]);
            }

            
        }        
    }
}
