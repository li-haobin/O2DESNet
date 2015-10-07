using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.Workshop
{
    internal abstract class Event : O2DESNet.Event
    {
        protected Simulator Simulator { get { return (Simulator)_simulator; } }
        protected Status Status { get { return Simulator.Status; } }
        protected Scenario Scenario { get { return Simulator.Scenario; } }
        internal Event(Simulator simulator) : base(simulator) { }
    }
}
