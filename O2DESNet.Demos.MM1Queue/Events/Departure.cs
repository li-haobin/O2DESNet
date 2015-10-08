using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.MM1Queue
{
    internal class Departure : Event
    {
        internal Customer Customer { get; private set; }
        internal Departure(Simulator sim, Customer customer) : base(sim) { Customer = customer; }
        public override void Invoke()
        {
            if (_sim.Status.WaitingQueue.Count > 0) new StartService(_sim, _sim.Status.WaitingQueue.Dequeue()).Invoke();
            else _sim.Status.Serving = null;
        }
    }
}
