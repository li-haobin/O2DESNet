using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.PathMover
{
    public class Job 
    {
        public ControlPoint Origin { get; set; }
        public ControlPoint Destination { get; set; }
        public Vehicle Vehicle { get; set; }

    }
}
