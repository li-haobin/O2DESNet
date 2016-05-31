using O2DESNet;
using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.PathMover
{
    public class JobFinish : Event<Scenario, Status>
    {
        public Job Job { get; set; }
        public override void Invoke()
        {
            Status.PM.PutOff(Job.Vehicle);
            Log("{0}\tJob Finished!", ClockTime.ToLongTimeString());
        }
    }
}
