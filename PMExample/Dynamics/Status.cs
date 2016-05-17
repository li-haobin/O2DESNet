using O2DESNet;
using O2DESNet.PathMover;
using PMExample.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample
{
    public class Status : Status<Scenario>
    {
        public PMDynamics PM { get; private set; }

        internal Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            PM = new PMDynamics(Scenario.PM);
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
