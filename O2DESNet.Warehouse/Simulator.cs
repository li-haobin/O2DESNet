using O2DESNet.Warehouse.Dynamics;
using O2DESNet.Warehouse.Statics;
using System;

namespace O2DESNet.Warehouse
{
    [Serializable]
    public class Simulator : O2DES
    {
        internal Scenario Scenario { get; private set; }
        internal Status Status { get; private set; }
        static internal Random RS { get; private set; } // use multiple random streams if necessary

        public Simulator(Scenario scenario, int seed = 0)
        {
            Scenario = scenario;
            Status = new Status(this);
            RS = new Random(seed);

            new Events.StartSim(this).Invoke();
        }
    }
}
