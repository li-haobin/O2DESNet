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
        public Job Next { get; set; }
        public Vehicle Vehicle { get; set; }
        protected override void Invoke()
        {
            Log("{0}\tJob Finished on {1} at {2}!", ClockTime.ToLongTimeString(), Vehicle, Vehicle.Current);

            Vehicle.Targets = new List<ControlPoint> { Next.Origin };
            Vehicle.OnCompletion = () => { Execute(new JobStart { Job = Next, Vehicle = Vehicle }); };
            
            Execute(new Move { Dynamics = Status.PM, Vehicle = Vehicle });
        }
    }
}
