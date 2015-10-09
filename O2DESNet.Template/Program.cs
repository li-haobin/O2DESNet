using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Template
{
    class Program
    {
        static void Main(string[] args)
        {
            var scenario = new Scenario(); // preparing the scenario
            var sim = new Simulator(scenario, seed: 0); // construct the simulator
            sim.Run(10000); // run simulator
            // sim.Status... // read analytics from status class
        }
    }
}
