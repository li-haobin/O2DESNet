using O2DESNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.PathMover
{
    public class Status : Status<Scenario>
    {
        public O2DESNet.PathMover.Dynamics PM { get; private set; }

        internal Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            PM = new O2DESNet.PathMover.Dynamics(Scenario.PM);
        }

        public override void WarmedUp(DateTime clockTime)
        {
            PM.WarmedUp(clockTime);
        }

        public Job CreateJob()
        {
            return new Job
            {
                Origin = PM.Statics.ControlPoints[DefaultRS.Next(PM.Statics.ControlPoints.Count)],
                Destination = PM.Statics.ControlPoints[DefaultRS.Next(PM.Statics.ControlPoints.Count)],
            };
        }

    }
}
