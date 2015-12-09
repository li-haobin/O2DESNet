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
            throw new NotImplementedException();
        }

        public override void Backtrack()
        {
            throw new NotImplementedException();
        }
    }
}
