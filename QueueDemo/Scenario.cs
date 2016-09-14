using QueueDemo.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace QueueDemo
{
    /// <summary>
    /// The Scenario class that specifies what to simulate
    /// </summary>
    public class Scenario : O2DESNet.Scenario
    {
        public Dynamics.Queue queue { get; set; }
        public Dynamics.Server server { get; set; }

        public Scenario()
        {
            queue = new Dynamics.Queue();
            server = new Dynamics.Server(TimeSpan.FromSeconds(30));
        }
    }
}
