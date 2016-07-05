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
        public List<Vehicle> Vehicles { get; private set; }

        internal Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            PM = new PMDynamics(Scenario.PM);
            Vehicles = new List<Vehicle>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            PM.WarmedUp(clockTime);
        }

        public Job CreateJob(Random rs)
        {
            if (rs.NextDouble() < Scenario.DischargingRatio) return new Job
            {
                Origin = Scenario.QuayPoints[rs.Next(Scenario.QuayPoints.Length)],
                Destination = Scenario.YardPoints[rs.Next(Scenario.YardPoints.Length)]
            };
            else return new Job
            {
                Origin = Scenario.YardPoints[rs.Next(Scenario.YardPoints.Length)],
                Destination = Scenario.QuayPoints[rs.Next(Scenario.QuayPoints.Length)]
            };

        }

    }
}
