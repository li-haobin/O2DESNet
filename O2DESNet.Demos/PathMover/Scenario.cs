using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.PathMover
{
    public class Scenario : O2DESNet.Scenario
    {
        public Statics PM { get; set; }
        public double JobHourlyRate { get; set; }
    }
}
