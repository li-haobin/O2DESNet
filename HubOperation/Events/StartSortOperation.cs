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
        public bool isSmallContainer(Dynamics.Container container)
        {
            return container.Type == "S";
        }
        public bool idleStation(Dynamics.InputStation station)
        {
            return station.isIdle;
        }
        public int idleStationCount()
        {
            return Scenario.StationsList.FindAll(idleStation).Count;
        }
        public bool isLargeContainer(Dynamics.Container container)
        {
            return container.Type == "L";
        }
        public int LargeToSmall(Dynamics.Container con1, Dynamics.Container con2)
        {
            if (con1.Type == "S" && con2.Type == "L")
            {
                return 1;
            }
            else if (con1.Type == "L" && con2.Type == "S")
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        public override void Invoke()
        {

            for (int i = 0; i < Scenario.ReadyContainersList.Count; i++)
            {
                Scenario.ReadyContainersList[i].ReadyTime = ClockTime;
            }


            //schedule start of unloading for all stations at current time (Start of sorting operation)
            int j = 0;
            int k = 0;
            Scenario.ReadyContainersList.Sort(LargeToSmall);
            while (j < Scenario.StationsList.Count)
            {

                if (Scenario.ReadyContainersList[k].Type == "L" && idleStationCount() >= 2)
                {
                    Dynamics.Container nextContainer = Scenario.ReadyContainersList[k];
                    Schedule(new StartUnloadPackages(nextContainer, Scenario.StationsList[j], Scenario.StationsList[j+1]), TimeSpan.Zero);
                    nextContainer.isUnloading = true;
                    j += 2;
                }
                else
                {
                    Dynamics.Container nextContainer = Scenario.ReadyContainersList[k];
                    Schedule(new StartUnloadPackages(nextContainer, Scenario.StationsList[j]), TimeSpan.Zero);
                    nextContainer.isUnloading = true;
                    j++;
                }
                k++;
            }

            Status.StartTime = ClockTime;
        }
    }
}
