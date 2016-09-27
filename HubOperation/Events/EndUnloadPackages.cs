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

        private static bool notUnloadedSmall(Dynamics.Container container)
        {
            return container.PackagesList.Any() && !container.isUnloading && (container.Type == "S");
        }
        private static bool notUnloadedLarge(Dynamics.Container container)
        {
            return container.PackagesList.Any() && !container.isUnloading && (container.Type == "L");
        }
        private static bool notEmpty(Dynamics.Container container)
        {
            return container.PackagesList.Any();
        }

        private bool partnerStationIdle()
        {
            if (Station.StationID % 2 == 0)
            {
                return Scenario.StationsList[Station.StationID - 2].isIdle;
            }
            else
            {
                return Scenario.StationsList[Station.StationID].isIdle;
            }

        }

        private Dynamics.InputStation partnerStation()
        {
            if (Station.StationID % 2 == 0)
            {
                return Scenario.StationsList[Station.StationID - 2];
            }
            else
            {
                return Scenario.StationsList[Station.StationID];
            }

        }
        private void ScheduleUnloadNextSmall()
        {
            Dynamics.Container nextContainer = Scenario.ReadyContainersList.Find(notUnloadedSmall);
            Schedule(new StartUnloadPackages(nextContainer, Station), TimeSpan.Zero);
            nextContainer.isUnloading = true;
        }

        private void ScheduleUnloadNextLarge()
        {
            Dynamics.Container nextContainer = Scenario.ReadyContainersList.Find(notUnloadedLarge);
            Schedule(new StartUnloadPackages(nextContainer, Station, partnerStation()), TimeSpan.Zero);
            nextContainer.isUnloading = true;

        }
        public override void Invoke()
        {

            if (Container.isEmpty == false)
            {
                Container.FinishUnloadingTime = ClockTime;
                Scenario.EmptiedContainersList.Add(Container);

                Status.ContainersSorted++;

                Container.isUnloading = false;
                Container.isEmpty = true;
            }
            // checks if there are still full containers and schedule their unloading
            if (Scenario.ReadyContainersList.Exists(notUnloaded))
            {

                //if current containers was Small
                if (Container.Type == "S")
                {
                    //if there are still small containers
                    if (Scenario.ReadyContainersList.Exists(notUnloadedSmall))
                    {
                        ScheduleUnloadNextSmall();
                    }
                    else
                    {
                        //unload large containers
                        if (partnerStationIdle())
                        {
                            ScheduleUnloadNextLarge();
                        }
                        else
                        {
                            Station.isIdle = true;
                        }
                    }

                }
                else
                {

                    if (Scenario.ReadyContainersList.Exists(notUnloadedLarge))
                    {
                        if (partnerStationIdle())
                        {
                            ScheduleUnloadNextLarge();
                        }
                        else
                        {
                            Station.isIdle = true;
                        }
                    }
                    else
                    {
                        ScheduleUnloadNextSmall();
                    }
                }
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
