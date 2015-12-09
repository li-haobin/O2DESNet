using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Events
{
    internal class StartPick : Event
    {
        internal Picker picker { get; private set; }

        internal StartPick(Simulator sim, Picker picker) : base(sim)
        {
            this.picker = picker;
        }
        public override void Invoke()
        {
            // Set Start Location
            picker.CurLocation = _sim.Scenario.StartCP; 
            if(picker.PickList.Count > 0)
            {
                var duration = picker.GetTravelTime(picker.PickList.First().location);
                _sim.ScheduleEvent(new ArriveLocation(_sim, picker), _sim.ClockTime.AddSeconds(duration));

                // Any status updates?
            }
            else
            {
                // Picklist empty
            }
        }

        public override void Backtrack()
        {
            throw new NotImplementedException();
        }
    }
}
