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

            for (int i = 1; i <= packagesCount; i++)
            {
                Dynamics.Package newPackage = new Dynamics.Package(route);
                container.PackagesList.Add(newPackage);

            }


            return container;
        }

        public void initDeliveryVansList()
        {
            List<Dynamics.Container> AllContainersList = new List<Dynamics.Container>();
            ReadyContainersList.ForEach(c => AllContainersList.Add(c));
            InboundContainersList.ForEach(c => AllContainersList.Add(c));

            for (int i = 0; i < AllContainersList.Count; i++)
                for (int j = 0; j < AllContainersList[i].PackagesList.Count; j++)
                {
                    Dynamics.Package newPackage = AllContainersList[i].PackagesList[j];
                    string route = newPackage.RouteID;

                if (route != "Transhipment")
                    {

                        if (!RoutesList.Contains(route))
                        {
                            RoutesList.Add(route);
                            DeliveryVansList.Add(new Dynamics.DeliveryVan(route));
                        }

                        addPackageToVanRoute(newPackage, route);

                    }
                }
        }

        public Scenario()
        {

            // initialize packages 
            //first wave containers -> ReadyContainersList
            //second wave containers -> InboundContainersList
            Dynamics.Container container1 = initPackagesToContainer(new Dynamics.LargeContainer(), 50, "Route1");
            initPackagesToContainer(container1, 40, "Route2");
            ReadyContainersList.Add(new Dynamics.LargeContainer(container1));
            ReadyContainersList.Add(new Dynamics.LargeContainer(container1));
            ReadyContainersList.Add(new Dynamics.LargeContainer(container1));

            Dynamics.Container container2 = initPackagesToContainer(new Dynamics.SmallContainer(), 15, "Transhipment");
            initPackagesToContainer(container2, 30, "Route2");
            ReadyContainersList.Add(new Dynamics.SmallContainer(container2));
            ReadyContainersList.Add(new Dynamics.SmallContainer(container2));
            InboundContainersList.Add(new Dynamics.SmallContainer(container2));

            Dynamics.Container container3 = initPackagesToContainer(new Dynamics.LargeContainer(), 15, "Route1");
            initPackagesToContainer(container3, 80, "Route3");
            ReadyContainersList.Add(new Dynamics.LargeContainer(container3));
            ReadyContainersList.Add(new Dynamics.LargeContainer(container3));
            InboundContainersList.Add(new Dynamics.LargeContainer(container3));
            InboundContainersList.Add(new Dynamics.LargeContainer(container3));

            Dynamics.Container container4 = initPackagesToContainer(new Dynamics.SmallContainer(), 15, "Transhipment");
            initPackagesToContainer(container2, 30, "Route3");
            ReadyContainersList.Add(new Dynamics.SmallContainer(container4));
            ReadyContainersList.Add(new Dynamics.SmallContainer(container4));
            InboundContainersList.Add(new Dynamics.SmallContainer(container4));

            initDeliveryVansList();

            // initialize input stations and their rates
            for (int i = 1; i <= 8; i++)
            {
                StationsList.Add(new Dynamics.InputStation(i,TimeSpan.FromSeconds(8)));
            }

        }
        // encapsulate all static properties here
        // ...
    }
}
