using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueDemo.Events
{
    /// <summary>
    /// O2DESNet Event - StartService
    /// </summary>
    public class StartService : O2DESNet.Event<Scenario, Status>
    {
        public override void Invoke()
        {
            Scenario.server.IsIdle = false;
            Schedule(new EndService(), Scenario.server.SvcTime);
        }
    }
}
