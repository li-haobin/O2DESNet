using System;
using O2DESNet.Components;
using QueueExample.Dynamics;

namespace QueueExample
{
    public class Status : O2DESNet.Status<Scenario>
    {
        public Queue<Customer> Queue { get; private set; }
        public Server<Customer> Server { get; private set; }

        public Status(Scenario scenario, int seed=0):base(scenario, seed)
        {
            Queue = new Queue<Customer>();
            Server = new Server<Customer>(1);
        }

        public override void WarmedUp(DateTime clockTime)
        {
            Queue.WarmedUp(clockTime);
            Server.WarmedUp(clockTime);
        }
    }
}
