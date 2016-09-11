using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace Test
{
    /// <summary>
    /// The simulator class
    /// </summary>
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            Execute(Status.Generator.Start());
        }
    }
}
