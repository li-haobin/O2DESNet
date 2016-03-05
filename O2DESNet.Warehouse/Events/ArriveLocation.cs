﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Events
{
    [Serializable]
    internal class ArriveLocation : Event
    {
        internal Picker picker { get; private set; }

        internal ArriveLocation(Simulator sim, Picker picker) : base(sim)
        {
            this.picker = picker;
        }
        public override void Invoke()
        {
            picker.CurLocation = picker.PickJobsToComplete.First().rack.OnShelf.BaseCP;
            var duration = picker.GetPickingTime();
            _sim.ScheduleEvent(new PickItem(_sim, picker), _sim.ClockTime.Add(duration));
        }

        public override void Backtrack()
        {
            throw new NotImplementedException();
        }
    }
}