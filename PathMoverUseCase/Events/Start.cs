using O2DESNet;
using O2DESNet.PathMover.Events;
using O2DESNet.PathMover.Statics;
using PathMoverUseCase.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathMoverUseCase.Events
{
    internal class Start : Event<Scenario, Status>
    {
        public Job Job { get; set; }

        protected override void Invoke()
        {
            var v = Status.PM1.PutOn(Job.Origin, ClockTime);
            v.Targets = new List<ControlPoint> { Job.Destination };
            v.OnCompletion = () => { Execute(new Finish { Job = Job }); };
            ((Simulator)Simulator).PM1.Schedule(new Move { Vehicle = v }, ClockTime);
        }
    }
}
