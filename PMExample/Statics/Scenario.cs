using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample
{
    public class Scenario : O2DESNet.Scenario
    {
        public PMStatics PM { get; set; }
        public double JobHourlyRate { get; set; }
    }
}
