using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Template
{
    internal class MyEvent1 : Event
    {
        // include dynamic entities, i.e., loads, involving in the event

        // internal Load_1 load_1 { get; private set; }
        // internal Load_2 load_1 { get; private set; }
        // ...

        internal MyEvent1(Simulator sim) : base(sim)
        {
            // also include loads in the constructor
        }
        public override void Invoke()
        {
            // updates _sim.Status if necessary

            // schedule subsequent events if necessary
            // _sim.ScheduleEvent(new MyEvent2(_sim, new Load_2()), TimeSpan.FromHours(some_random_value));
        }
    }
}
