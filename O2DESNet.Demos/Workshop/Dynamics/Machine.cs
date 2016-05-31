using O2DESNet.Demos.Workshop.Statics;
using System.Collections.Generic;

namespace O2DESNet.Demos.Workshop.Dynamics
{
    public class Machine
    {
        public int Id { get; internal set; }
        public WorkStation WorkStation { get; internal set; }
        public List<Product> Processing { get; set; }
        public bool IsIdle { get { return Processing == null; } }
    }
}
