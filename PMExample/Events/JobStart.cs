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
    public class JobStart : Event<Scenario, Status>
    {
        public Job Job { get; set; }
        public Vehicle Vehicle { get; set; }
        protected override void Invoke()
        {
            Vehicle.Targets = new List<ControlPoint> { Job.Destination };
            Vehicle.OnCompletion = () => { Execute(new JobFinish { Next = Status.CreateJob(DefaultRS), Vehicle = Vehicle }); };

            Log("{0}\tJob Started on {1} at {2}!", ClockTime.ToLongTimeString(), Vehicle, Vehicle.Current);
            Execute(new Move { Dynamics = Status.GridStatus, Vehicle = Vehicle });
        }
    }
}
