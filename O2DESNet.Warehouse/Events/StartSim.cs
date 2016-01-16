using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Events
{
    [Serializable]
    internal class StartSim : Event
    {
        public StartSim(Simulator sim) : base(sim) { }

        public override void Invoke()
        {
            foreach (var picker in _sim.Scenario.AllPickers)
            {
                picker.CurLocation = _sim.Scenario.StartCP;
                _sim.ScheduleEvent(new StartPick(_sim, picker), _sim.ClockTime);
            }
        }

        public override void Backtrack()
        {
            throw new NotImplementedException("Unable to Backtrack Start Simulation Event");
        }
    }
}
