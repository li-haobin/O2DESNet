using O2DESNet;
using PathMoverUseCase.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathMoverUseCase.Events
{
    internal class Finish : Event<Scenario, Status>
    {
        public Job Job { get; set; }

        protected override void Invoke()
        {
           
        }
    }
}
