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
        public Dynamics.InputStation Station;
        public Predicate<Dynamics.Container> nonEmpty = notUnloaded;       

        public EndUnloadPackages(Dynamics.Container container, Dynamics.InputStation station)
        {
            container.isEmpty = true;
            container.isUnloading = false;
            Station = station;
        }


        /// <summary>
        /// for predicate<> assignment
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private static bool notUnloaded(Dynamics.Container container)
        {
            return !container.isEmpty && !container.isUnloading;
        }

        private static bool notEmpty(Dynamics.Container container)
        {
            return !container.isEmpty;
        }

        public override void Invoke()
        {

            if (Scenario.ContainersList.Exists(notUnloaded))
            {
                Dynamics.Container nextContainer = Scenario.ContainersList.Find(notUnloaded);
                Schedule(new StartUnloadPackages(nextContainer, Station), TimeSpan.Zero);
            }
            else
            {
                Station.isIdle = true;
                if (!Scenario.ContainersList.Exists(notEmpty))
                {
                    Status.EndTime = ClockTime;
                }
            }
            Status.ContainersSorted++;

        }
    }
}
