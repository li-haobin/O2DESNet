using O2DESNet;
using System;
using System.Collections.Generic;

namespace QueueExample
{
    public class Status : Status<Scenario>
    {
        public O2DESNet.Queue<Load> Queue { get; private set; }
        public Processor<Load> Server { get; private set; }
        public List<Load> Processed { get; private set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            Queue = new O2DESNet.Queue<Load>();
            Server = new Processor<Load>(1);
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
