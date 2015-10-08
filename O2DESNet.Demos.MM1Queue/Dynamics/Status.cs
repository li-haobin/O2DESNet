using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.MM1Queue
{
    internal class Status
    {
        private Simulator _sim;
        public Queue<Customer> WaitingQueue { get; internal set; }
        public Customer Serving { get; internal set; }

        internal Status(Simulator simulation)
        {
            _sim = simulation;
            WaitingQueue = new Queue<Customer>();
            Serving = null;
        }
    }
}
