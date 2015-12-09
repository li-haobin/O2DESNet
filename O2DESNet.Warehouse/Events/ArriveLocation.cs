using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Events
{
    internal class ArriveLocation : Event
    {
        internal Picker picker { get; private set; }

        internal ArriveLocation(Simulator sim, Picker picker) : base(sim)
        {
            this.picker = picker;
        }
        public override void Invoke()
        {
            picker.CurLocation = picker.PickList.First().location;
            var duration = picker.GetPickingTime();
            _sim.ScheduleEvent(new PickItem(_sim, picker), _sim.ClockTime.Add(duration));
        }

        public override void Backtrack()
        {
            throw new NotImplementedException();
        }
    }
}
