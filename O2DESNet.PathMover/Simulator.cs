using O2DESNet.PathMover.Dynamics;
using O2DESNet.PathMover.Statics;
using System;

namespace O2DESNet.PathMover
{
    public class Simulator : O2DES
    {
        internal Scenario Scenario { get; private set; }
        internal Status Status { get; private set; }
        internal Random RS { get; private set; } // use multiple random streams if necessary

        public Simulator(Scenario scenario, int seed)
        {
            Scenario = scenario;
            Status = new Status(this);
            RS = new Random(seed);

            new Events.Start(this).Invoke();
        }
    }
}
