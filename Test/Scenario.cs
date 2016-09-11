using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace Test
{
    /// <summary>
    /// The Scenario class that specifies what to simulate
    /// </summary>
    public class Scenario : O2DESNet.Scenario
    {
        public Func<Random, TimeSpan> InterArrivalTime { get; set; }
        public Func<Random, TimeSpan> ServiceTime { get; set; }
        public int ServerCapacity { get; set; }        
    }
}
