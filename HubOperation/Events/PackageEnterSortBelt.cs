using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Events
{
    /// <summary>
    /// O2DESNet Event - PackageEnterSortBelt
    /// </summary>
    public class PackageEnterSortBelt : O2DESNet.Event<Scenario, Status>
    {
        public Dynamics.Package Package;

        public PackageEnterSortBelt(Dynamics.Package package)
        {
            Package = package;
        }

        public override void Invoke()
        {
            if (Package.RouteID != "Transhipment")
            {
                Schedule(new PackageReachDeliveryVan(Package), TimeSpan.FromSeconds(30));
            }
        }
    }
}
