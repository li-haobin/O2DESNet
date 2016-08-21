using Test.Dynamics;
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
        public Generator<Scenario, Status, Load> Generator { get; private set; }
        public Queue<Scenario, Status, Load> Queue { get; private set; }
        public Server<Scenario, Status, Load> Server { get; private set; }

        public List<Load> Processed { get; private set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            Generator = new Generator<Scenario, Status, Load>();
            Queue = new Queue<Scenario, Status, Load>();
            Server = new Server<Scenario, Status, Load>(1);
            Processed = new List<Load>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            Queue.WarmedUp(clockTime);
            Server.WarmedUp(clockTime);
            Processed = new List<Load>();
        }
    }
}
