using O2DESNet.Warehouse.Dynamics;
using O2DESNet.Warehouse.Statics;
using System;

namespace O2DESNet.Warehouse
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
