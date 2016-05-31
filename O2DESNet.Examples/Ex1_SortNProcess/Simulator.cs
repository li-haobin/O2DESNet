using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Examples.Ex1_SortNProcess
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            Execute(new Arrive<Scenario, Status> { });
        }
    }
}
