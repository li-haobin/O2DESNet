using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace HubOperation.Events
{
    /// <summary>
    /// O2DESNet Event - MyEvent2
    /// </summary>
    public class StartUnloadPackages : O2DESNet.Event<Scenario, Status>
    {

        public Dynamics.Container Container;
        public Dynamics.InputStation FirstStation;
        public Dynamics.InputStation SecondStation;

        public StartUnloadPackages(Dynamics.Container container,Dynamics.InputStation station)
        {
            Container = container;
            FirstStation = station;
            SecondStation = null;
        }

        public StartUnloadPackages(Dynamics.Container container, Dynamics.InputStation station1, Dynamics.InputStation station2)
        {
            Container = container;
            FirstStation = station1;
            SecondStation = station2;
        }


        public override void Invoke()
        {
            if (SecondStation == null)
            {
                FirstStation.isIdle = false;

                // unload packages to enter sort belt at constant interval (currently), according to rate of input station
                int i = 0;
                while (Container.PackagesList.Any())
                {
                    Schedule(new PackageEnterSortBelt(Container.PackagesList[0]), FirstStation.SvcTime.Multiply(i + 1));
                    Container.PackagesList.RemoveAt(0);
                    i++;
                }
                TimeSpan LastUnloadingTime = FirstStation.SvcTime.Multiply(i);
                Schedule(new EndUnloadPackages(Container, FirstStation), LastUnloadingTime);
            }
            else
            {
                FirstStation.isIdle = false;
                SecondStation.isIdle = false;

                // unload packages to enter sort belt at constant interval (currently), according to rate of input station
                TimeSpan first = FirstStation.SvcTime;
                TimeSpan second = SecondStation.SvcTime;
                TimeSpan LastUnloadingTime = new TimeSpan();
                while (Container.PackagesList.Any())
                {
                    if (first <= second)
                    {
                        Schedule(new PackageEnterSortBelt(Container.PackagesList[0]), first);
                        Container.PackagesList.RemoveAt(0);
                        LastUnloadingTime = first;
                        first += FirstStation.SvcTime;
                    }
                    else
                    {
                        Schedule(new PackageEnterSortBelt(Container.PackagesList[0]), second);

                        Container.PackagesList.RemoveAt(0);
                        LastUnloadingTime = second;
                        second += SecondStation.SvcTime;
                    }
                    
                }

                Schedule(new EndUnloadPackages(Container, FirstStation), LastUnloadingTime);
                Schedule(new EndUnloadPackages(Container, SecondStation), LastUnloadingTime);


            }


        }
    }
}
