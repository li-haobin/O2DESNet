using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.Workshop
{
    internal class Initialization : Event
    {
        internal Initialization(Simulator simulator) : base(simulator)
        {
            Invoke = () =>
            {
                ScheduleEvent(new Arrival(simulator), Scenario.Generate_InterArrivalTime(Status.RS));
            };
        }
    }
}
