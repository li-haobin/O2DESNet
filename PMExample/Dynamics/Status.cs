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
        public GridStatus GridStatus { get; private set; }
        public List<Vehicle> Vehicles { get; private set; }
        public int JobsCount { get; set; }

        internal Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            GridStatus = new GridStatus(Scenario.Grid);
            Vehicles = new List<Vehicle>();
            JobsCount = 0;
        }

        public override void WarmedUp(DateTime clockTime)
        {
            GridStatus.WarmedUp(clockTime);
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
