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
                if (i % 3 == 0)
                {  
                    packagesList1.Add(new Dynamics.Package("Route1"));
                }
                else
                {
                    packagesList1.Add(new Dynamics.Package("Route2"));
                }
            }
            packagesList1.Add(new Dynamics.Package("Transhipment"));

            for (int i = 1; i <= 12; i++)
            {
                Scenario.ContainersList.Add(new Dynamics.Container(ClockTime, packagesList1));
            }      

            for (int i = 1; i <= 4; i++)
            {
                Scenario.StationsList.Add(new Dynamics.InputStation(TimeSpan.FromSeconds(8)));
            }

            List<Dynamics.Package> packagesList2 = new List<Dynamics.Package>();
            for (int i = 1; i <= Scenario.ContainersList.Count * 6; i++)
            {
                    packagesList2.Add(new Dynamics.Package("Route1"));
            }
            List<Dynamics.Package> packagesList3 = new List<Dynamics.Package>();
            for (int i = 1; i <= Scenario.ContainersList.Count * 14 ; i++)
            {
                packagesList3.Add(new Dynamics.Package("Route2"));
            }

            Scenario.DeliveryVansList.Add(new Dynamics.DeliveryVan( packagesList2, "Route1" ));
            Scenario.DeliveryVansList.Add(new Dynamics.DeliveryVan( packagesList3, "Route2" ));


            for (int i = 0; i < Scenario.StationsList.Count; i++)
            {
                Schedule(new StartUnloadPackages(Scenario.ContainersList[i], Scenario.StationsList[i]), TimeSpan.Zero); ;
                Scenario.ContainersList[i].isUnloading = true;
            }


            Status.StartTime = ClockTime;
        }
    }
}
