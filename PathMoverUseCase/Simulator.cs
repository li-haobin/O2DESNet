using O2DESNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathMoverUseCase
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator<O2DESNet.PathMover.Scenario, O2DESNet.PathMover.Status> PM1 { get; private set; }
        public Simulator(Status status) : base(status)
        {
            PM1 = new O2DESNet.PathMover.Simulator(status.PM1);
        }

        protected override bool ExecuteHeadEvent()
        {
            bool result = false;
            if (PM1.HeadEventTime <= HeadEventTime)
            {
                result |= PM1.Run(1);
                ClockTime = PM1.ClockTime;
            }
            if (!result)
            {
                result |= base.ExecuteHeadEvent();
                PM1.ClockTime = ClockTime;
            }
            return true;
        }
    }
}
