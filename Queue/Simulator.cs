using O2DESNet.Components;
using System;

namespace Queue
{
    public class Scenario : O2DESNet.Scenario { }

    public class Customer { }

    public class Status : O2DESNet.Status<Scenario>
    {
        public Queue<Customer> Queue { get; private set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            Queue = new Queue<Customer>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            Queue.WarmedUp(clockTime);
        }
    }

    public class Simulator : O2DESNet.Simulator<Scenario, Status>
    {
        public Simulator()
        {

        }
    }
}
