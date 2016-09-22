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
        public Dynamics.InputStation Station;

        public StartUnloadPackages(Dynamics.Container container,Dynamics.InputStation station)
        {
            Container = container;
            Station = station;
        }

        public override void Invoke()
        {
            Station.isIdle = false;
            

            int i = 0;
            while (Container.PackagesList.Any())
            {
             Schedule(new PackageEnterSortBelt(Container.PackagesList[0]), Station.SvcTime.Multiply(i+1));
                Container.PackagesList.RemoveAt(0);
                i++;
            }
            TimeSpan LastUnloadingTime = Station.SvcTime.Multiply(i);

            Schedule(new EndUnloadPackages(Container,Station), LastUnloadingTime);

        }
    }
}
