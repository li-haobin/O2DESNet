using O2DESNet;
using O2DESNet.PathMover;
using PMExample.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample.Events
{
    public class JobFinish : Event<Scenario, Status>
    {
        public Job Job { get; set; }
        protected override void Invoke()
        {
            Status.PM.PutOff(Job.Vehicle);
            Log("{0}\tJob Finished!", ClockTime.ToLongTimeString());
        }
    }
}
