using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.Workshop
{
    internal class Arrival : Event
    {
        internal Arrival(Simulator simulator) : base(simulator)
        {
            Invoke = () =>
            {
                var job = Status.Generate_EnteringJob();
                Process(job)();
                ScheduleEvent(new Arrival(Simulator), Scenario.Generate_InterArrivalTime(Status.RS));
            };
        }
    }
}
