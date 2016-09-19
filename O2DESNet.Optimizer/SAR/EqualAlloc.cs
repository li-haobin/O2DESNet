using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public class EqualAlloc : SAR
    {
        public override Dictionary<DenseVector, int> Alloc(int budget, IEnumerable<StochasticSolution> solutions)
        {
            return Alloc(budget, solutions,
                getTargetRatios: sols => Enumerable.Repeat(1.0, sols.Length).ToArray());
        }
    }
}
