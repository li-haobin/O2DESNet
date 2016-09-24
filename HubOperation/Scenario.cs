using HubOperation.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation
{
    /// <summary>
    /// The Scenario class that specifies what to simulate
    /// </summary>
    public class Scenario : O2DESNet.Scenario
    {
        public List<Dynamics.Container> ReadyContainersList = new List<Dynamics.Container>();

        public List<Dynamics.Container> InboundContainersList = new List<Dynamics.Container>();

        public List<Dynamics.Container> EmptiedContainersList = new List<Dynamics.Container>();

        public List<Dynamics.InputStation> StationsList = new List<Dynamics.InputStation>();

        public List<Dynamics.DeliveryVan> DeliveryVansList = new List<Dynamics.DeliveryVan>();

        public List<string> RoutesList = new List<string>();

        public Predicate<Dynamics.DeliveryVan> isRoute(string route)
        {
            return delegate (Dynamics.DeliveryVan van)
                {
                    return van.Route == route;
                };
        }
        public void addPackageToVanRoute(Dynamics.Package package, string route)
        {

            Dynamics.DeliveryVan van = DeliveryVansList.Find(isRoute(route));
            van.PackagesDeliveryList.Add(package);

        }

        public Dynamics.Container initPackagesToContainer(Dynamics.Container container, int packagesCount, string route)
        {
            if (!RoutesList.Contains(route) && route != "Transhipment")
            {
                RoutesList.Add(route);
                DeliveryVansList.Add(new Dynamics.DeliveryVan(route));
            }

            for (int i = 1; i <= packagesCount; i++)
            {
                Dynamics.Package newPackage = new Dynamics.Package(route);
                container.PackagesList.Add(newPackage);

                if (route != "Transhipment")
                {
                    addPackageToVanRoute(newPackage, route);
                }
            }


            return container;
        }

        public Scenario()
        {

            // initialize packages 
            //first wave containers -> ReadyContainersList
            //second wave containers -> InboundContainersList
            Dynamics.Container container1 = initPackagesToContainer(new Dynamics.Container(), 30, "Route1");
            initPackagesToContainer(container1, 10, "Route2");
            ReadyContainersList.Add(new Dynamics.Container(container1));
            ReadyContainersList.Add(new Dynamics.Container(container1));
            ReadyContainersList.Add(new Dynamics.Container(container1));

            Dynamics.Container container2 = initPackagesToContainer(new Dynamics.Container(), 15, "Transhipment");
            initPackagesToContainer(container2, 30, "Route2");
            ReadyContainersList.Add(new Dynamics.Container(container2));
            ReadyContainersList.Add(new Dynamics.Container(container2));
            InboundContainersList.Add(new Dynamics.Container(container2));

            Dynamics.Container container3 = initPackagesToContainer(new Dynamics.Container(), 15, "Route1");
            initPackagesToContainer(container3, 45, "Route3");
            InboundContainersList.Add(new Dynamics.Container(container3));
            InboundContainersList.Add(new Dynamics.Container(container3));

            // initialize input stations and their rates
            for (int i = 1; i <= 4; i++)
            {
                StationsList.Add(new Dynamics.InputStation(TimeSpan.FromSeconds(8)));
            }

        }
        // encapsulate all static properties here
        // ...
    }
}
