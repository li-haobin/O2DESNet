using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Events
{
    /// <summary>
    /// O2DESNet Event - AircraftContainersUnload
    /// </summary>
    public class AircraftContainersUnload : O2DESNet.Event<Scenario, Status>
    {
        public Dynamics.Container Container;

        public AircraftContainersUnload(Dynamics.Container container)
        {
            Container = container;
        }

        public bool idleStation(Dynamics.InputStation station)
        {
            return station.isIdle;
        }

        public override void Invoke()
        {
            Container.ReadyTime = ClockTime;
            Scenario.ReadyContainersList.Add(Container);
            if (Scenario.StationsList.Exists(idleStation))
            {
                Schedule(new StartUnloadPackages(Container, Scenario.StationsList.Find(idleStation)) , TimeSpan.Zero);
            }

        }
    }
}
