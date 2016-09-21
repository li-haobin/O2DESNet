using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Events
{
    /// <summary>
    /// O2DESNet Event - StartSortOperation
    /// </summary>
    public class StartSortOperation : O2DESNet.Event<Scenario, Status>
    {
        public override void Invoke()
        {
            List<Dynamics.Package> packagesList1 = new List<Dynamics.Package>();
            for (int i = 1; i <= 20; i++)
            {
                packagesList1.Add(new Dynamics.Package("Route1"));
            }

            for (int i = 1; i <= 4; i++)
            {
                Scenario.ContainersList.Add(new Dynamics.Container(ClockTime, packagesList1));
            }

            for (int i = 0; i < Scenario.StationsList.Count; i++)
            {
                Schedule(new StartUnloadPackages(Scenario.ContainersList[i], Scenario.StationsList[i]), TimeSpan.Zero); ;
            }

            Status.StartTime = ClockTime;
        }
    }
}
