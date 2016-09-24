using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Events
{
    /// <summary>
    /// O2DESNet Event - EndUnloadPackages
    /// </summary>
    public class EndUnloadPackages : O2DESNet.Event<Scenario, Status>
    {
        public Dynamics.Container Container;
        public Dynamics.InputStation Station;   

        public EndUnloadPackages(Dynamics.Container container, Dynamics.InputStation station)
        {

            Container = container;
            Station = station;
        }


        /// <summary>
        /// for predicate<> assignment
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private static bool notUnloaded(Dynamics.Container container)
        {
            return container.PackagesList.Any() && !container.isUnloading;
        }

        private static bool notEmpty(Dynamics.Container container)
        {
            return container.PackagesList.Any();
        }

        public override void Invoke()
        {
            Container.isEmpty = true;
            Container.isUnloading = false;
            Container.FinishUnloadingTime = ClockTime;
            Scenario.EmptiedContainersList.Add(Container);
            Status.ContainersSorted++;

            // checks if there are still full containers and schedule their unloading
            if (Scenario.ReadyContainersList.Exists(notUnloaded))
            {
                Dynamics.Container nextContainer = Scenario.ReadyContainersList.Find(notUnloaded);
                Schedule(new StartUnloadPackages(nextContainer, Station), TimeSpan.Zero);
                nextContainer.isUnloading = true;
            }
            else
            {
                Station.isIdle = true;

                if (!Scenario.ReadyContainersList.Exists(notEmpty) && Scenario.InboundContainersList.Count == 0)
                {
                    Status.UnloadPackagesEndTime = ClockTime;
                }

            }
            

        }
    }
}
