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

           

            for (int i = 0; i < Scenario.ReadyContainersList.Count; i++)
            {
                Scenario.ReadyContainersList[i].ReadyTime = ClockTime;
            }      


            //schedule start of unloading for all stations at current time (Start of sorting operation)
            for (int i = 0; i < Scenario.StationsList.Count; i++)
            {
                Schedule(new StartUnloadPackages(Scenario.ReadyContainersList[i], Scenario.StationsList[i]), TimeSpan.Zero); ;
                Scenario.ReadyContainersList[i].isUnloading = true;
            }


            Status.StartTime = ClockTime;
        }
    }
}
