using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Events
{
    [Serializable]
    internal class EndPick : Event
    {
        internal Picker picker { get; private set; }

        internal EndPick(Simulator sim, Picker picker) : base(sim)
        {
            this.picker = picker;
        }
        public override void Invoke()
        {
            // Check
            if (picker.PickListToComplete.Count > 0) throw new Exception("There are still items to pick!");

            // Just status update
            picker.CurLocation = _sim.Scenario.StartCP;
            picker.EndTime = _sim.ClockTime;
            picker.IsIdle = true;
            _sim.Status.CaptureCompletedPickList(picker);
            _sim.Status.DecrementActivePicker();

            if (_sim.Scenario.MasterPickList[picker.Type].Count > 0)
            {
                _sim.ScheduleEvent(new StartPick(_sim, picker), _sim.ClockTime + picker.Type.UnloadingTime);
            }
        }

        public override void Backtrack()
        {
            throw new NotImplementedException();
        }
    }
}
