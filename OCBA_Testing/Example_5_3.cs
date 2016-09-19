using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCBA_Testing
{
    public class Example_3_5 : O2DESNet.Optimizer.Benchmarks.RankingNSelection
    {
        public Example_3_5(int seed = 0) : base(nDesigns: 5, seed: seed)
        {
            TrueMeans = new double[] { 1, 2, 3, 4, 5 };
            TrueStdDevs = new double[] { 1, 1, 3, 3, 2 };
        }
    }
}
