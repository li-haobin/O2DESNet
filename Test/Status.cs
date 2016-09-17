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

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            GGnQueue = new GGnQueue<Scenario, Status, Load>(
                statics: Scenario,
                seed: DefaultRS.Next());
        }

        public override void WarmedUp(DateTime clockTime)
        {
            GGnQueue.WarmedUp(clockTime);
        }

        public override void WriteToConsole()
        {
            GGnQueue.WriteToConsole();
        }
    }
}
