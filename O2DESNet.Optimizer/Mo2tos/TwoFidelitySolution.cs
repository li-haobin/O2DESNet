using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public class TwoFidelitySolution : Solution
    {
        public DenseVector LowFidelityObjectives { get; private set; }

        public TwoFidelitySolution(DenseVector decisions, DenseVector lowFidelityObjectives) : base(decisions)
        {
            LowFidelityObjectives = lowFidelityObjectives;
        }
    }
}
