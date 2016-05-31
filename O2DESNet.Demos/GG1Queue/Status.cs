using O2DESNet;
using System;
using System.Collections.Generic;

namespace O2DESNet.Demos.GG1Queue
{
    public class Status : Status<Scenario>
    {
        public O2DESNet.Queue<Load> Queue { get; private set; }
        public Processor<Load> Processor { get; private set; }
        public List<Load> Processed { get; private set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            Queue = new O2DESNet.Queue<Load>();
            Processor = new Processor<Load>(1);
            Processed = new List<Load>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            Queue.WarmedUp(clockTime);
            Processor.WarmedUp(clockTime);
            Processed = new List<Load>();
        }
    }
}
