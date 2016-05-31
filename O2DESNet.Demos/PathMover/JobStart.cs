using O2DESNet;
using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.PathMover
{
    public class JobStart : Event<Scenario, Status>
    {
        public Job Job { get; set; }
        public override void Invoke()
        {
            Job.Vehicle = Status.PM.PutOn(Job.Origin, ClockTime);
            Job.Vehicle.Targets = new List<ControlPoint> { Job.Destination };
            Job.Vehicle.OnCompletion = () => { Execute(new JobFinish { Job = Job }); };

            Log("{0}\tJob Started!", ClockTime.ToLongTimeString());
            Schedule(new JobStart { Job = Status.CreateJob() }, TimeSpan.FromSeconds(DefaultRS.NextDouble() * 3600 / Scenario.JobHourlyRate));
            Execute(new Move { Dynamics = Status.PM, Vehicle = Job.Vehicle });
        }
    }
}
