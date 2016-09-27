using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Events
{
    /// <summary>
    /// O2DESNet Event - PackageUnload
    /// </summary>
    public class PackageUnload : O2DESNet.Event<Scenario, Status>
    {
        public Dynamics.Container Container;
        public Dynamics.InputStation Station;

        public PackageUnload(Dynamics.Container container, Dynamics.InputStation station)
        {
            Container = container;
            Station = station;
        }

        public override void Invoke()
        {
            Station.PackagesUnloaded++;
                Schedule(new PackageEnterSortBelt(Container.PackagesList[0]), Station.SvcTime);

        }
    }
}
