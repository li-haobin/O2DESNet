using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Events
{
    internal class PickItem : Event
    {
        internal Picker picker { get; private set; }

        internal PickItem(Simulator sim, Picker picker) : base(sim)
        {
            this.picker = picker;
        }
        public override void Invoke()
        {
            picker.PickNextItem();
            if (picker.PickList.Count > 0)
            {
                var shelfCP = picker.PickList.First().rack.OnShelf.BaseCP;
                var duration = picker.GetTravelTime(shelfCP);
                _sim.ScheduleEvent(new ArriveLocation(_sim, picker), _sim.ClockTime.AddSeconds(duration));

                // Any status updates?
            }
            else
            {
                // Return to start location
                var duration = picker.GetTravelTime(_sim.Scenario.StartCP);
                _sim.ScheduleEvent(new EndPick(_sim, picker), _sim.ClockTime.AddSeconds(duration));
            }
        }

        public override void Backtrack()
        {
            throw new NotImplementedException();
        }
    }
}
