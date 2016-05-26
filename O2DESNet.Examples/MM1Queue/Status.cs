using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Examples.MM1Queue
{
    public class Status : Status<Scenario>
    {
        public Queue<Load> Queue { get; private set; }
        public Processor<Load> Processor { get; private set; }
        public List<Load> Processed { get; private set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            Queue = new Queue<Load>();
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
