using O2DESNet;
using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.PathMover
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            for (int i = 0; i < Scenario.NumVehicles; i++)
            {
                var job = Status.CreateJob(DefaultRS);
                var vehicle = Status.GridStatus.PutOn(job.Origin, ClockTime);
                Status.Vehicles.Add(vehicle);
                Schedule(new JobStart { Job = job, Vehicle = vehicle }, ClockTime);
            }
        }
    }
}
