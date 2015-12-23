using System;

namespace O2DESNet
{
    public abstract class Status<TScenario> where TScenario : Scenario
    {
        internal protected TScenario Scenario { get; private set; }
        internal protected Random DefaultRS { get; private set; }
        public int Seed { get { return Seed; } set { Seed = value; DefaultRS = new Random(Seed); } }
        public Status(TScenario scenario, int seed = 0)
        {
            Scenario = scenario;
            Seed = seed;
        }
    }
}
