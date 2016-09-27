using HubOperation.Dynamics;
using O2DESNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation
{
    /// <summary>
    /// The Status class that provides a snapshot of simulated system in run-time
    /// </summary>
    public class Status : Status<Scenario>
    {
        // encapsulate all dynamic properties here
        // ...
        public DateTime StartTime;
        public DateTime LastVanReadyTime;
        public DateTime UnloadPackagesEndTime;
        public int ContainersSorted;
        public int PackagesOnSortBelt;

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            // initialize all the dynamic elements
            // ...            
            StartTime = new DateTime();
            LastVanReadyTime = new DateTime();
            UnloadPackagesEndTime = new DateTime();
            ContainersSorted = 0;
            PackagesOnSortBelt = 0;

        }

        // implement methods that help to update the Status
        // ...

        public TimeSpan getPackagesUnloadTime()
        {
            return UnloadPackagesEndTime - StartTime;

        }

        public TimeSpan getLastReadyVanTime()
        {
            return LastVanReadyTime - StartTime;
        }
    }
}
