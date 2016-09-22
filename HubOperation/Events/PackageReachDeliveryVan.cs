using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Events
{
    /// <summary>
    /// O2DESNet Event - PackageReach
    /// </summary>
    public class PackageReachDeliveryVan : O2DESNet.Event<Scenario, Status>
    {
        public Dynamics.Package Package;

        public PackageReachDeliveryVan(Dynamics.Package package)
        {
            Package = package;
        }


        private bool matchRoute(Dynamics.DeliveryVan van)
        {
            return van.Route == Package.RouteID;
        }
        public override void Invoke()
        {
            Dynamics.DeliveryVan loadDeliveryVan = Scenario.DeliveryVansList.Find(matchRoute);
            loadDeliveryVan.PackagesLoaded.Add(Package);
            if(loadDeliveryVan.CheckIfFullyLoaded())
            {
                loadDeliveryVan.DeliveryReadyTime = ClockTime;
                Status.LastVanReadyTime = ClockTime;
            }
        }
    }
}
