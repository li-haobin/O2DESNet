using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Events
{
    /// <summary>
    /// O2DESNet Event - AircraftArrival
    /// </summary>
    public class AircraftArrival : O2DESNet.Event<Scenario, Status>
    {
        public override void Invoke()
        {
            TimeSpan delay = TimeSpan.Zero;
            int unloadCount = 0;
            while (Scenario.InboundContainersList.Count > 0)
            {
                unloadCount++;
                Schedule(new AircraftContainersUnload(Scenario.InboundContainersList[0]), delay);
                Scenario.InboundContainersList.RemoveAt(0);

                // two containers can be unloaded together, every 4min
                if (unloadCount == 2)
                {
                    unloadCount = 0;
                    delay = delay + TimeSpan.FromMinutes(4);
                }
            }

        }
    }
}
