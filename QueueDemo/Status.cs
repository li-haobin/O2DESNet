using QueueDemo.Dynamics;
using O2DESNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueDemo
{
    /// <summary>
    /// The Status class that provides a snapshot of simulated system in run-time
    /// </summary>
    public class Status : Status<Scenario>
    {
        // encapsulate all dynamic properties here
        // ...
        public TimeSpan WaitingTime { get; set; }
        public int ServeCount { get; set; }
        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            // initialize all the dynamic elements
            // ...            
            WaitingTime = TimeSpan.Zero;
            ServeCount = 0;

        }

        public TimeSpan getAverageWait()
        {
           return TimeSpan.FromTicks(WaitingTime.Ticks / ServeCount);
        }
        // implement methods that help to update the Status
        // ...
    }
}
