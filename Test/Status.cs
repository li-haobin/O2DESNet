using O2DESNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    /// <summary>
    /// The Status class that provides a snapshot of simulated system in run-time
    /// </summary>
    public class Status : Status<Scenario>
    {
        public GGnQueue<Scenario, Status, Load> GGnQueue { get; private set; }
        public List<Load<Scenario, Status>> Processed { get; private set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {            
            Processed = new List<Load<Scenario, Status>>();
            GGnQueue = new GGnQueue<Scenario, Status, Load>(
                interArrivalTime: Scenario.InterArrivalTime,
                create: () => new Load(),
                serviceTime: Scenario.ServiceTime,
                serverCapacity: Scenario.ServerCapacity,
                seed: DefaultRS.Next());

            GGnQueue.OnDepart.Add(load => new Archive(load));
        }

        public override void WarmedUp(DateTime clockTime)
        {
            GGnQueue.WarmedUp(clockTime);
            Processed = new List<Load<Scenario, Status>>();
        }
    }
}
