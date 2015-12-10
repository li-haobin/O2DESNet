using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Events
{
    internal class EndPick : Event
    {
        internal Picker picker { get; private set; }

        internal EndPick(Simulator sim, Picker picker) : base(sim)
        {
            this.picker = picker;
        }
        public override void Invoke()
        {
            // Just status update
            picker.CurLocation = _sim.Scenario.StartCP;
            picker.EndTime = _sim.ClockTime;
            picker.IsIdle = true;
        }

        public override void Backtrack()
        {
            throw new NotImplementedException();
        }
    }
}
