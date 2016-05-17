using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathMoverUseCase.Dynamics
{
    public class Job
    {
        public ControlPoint Origin { get; set; }
        public ControlPoint Destination { get; set; }
    }
}
