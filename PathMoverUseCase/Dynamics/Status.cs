using PathMoverUseCase.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathMoverUseCase
{
    public class Status : O2DESNet.Status<Scenario>
    {
        public O2DESNet.PathMover.Status PM1 { get; set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            PM1 = new O2DESNet.PathMover.Status(Scenario.PM1, DefaultRS.Next());
        }

        public override void WarmedUp(DateTime clockTime)
        {
            PM1.WarmedUp(clockTime);
        }

        public Job CreateJob()
        {
            return new Job
            {
                Origin = Scenario.PM1.ControlPoints[DefaultRS.Next(Scenario.PM1.ControlPoints.Count)],
                Destination = Scenario.PM1.ControlPoints[DefaultRS.Next(Scenario.PM1.ControlPoints.Count)]
            };
        }
    }
}
