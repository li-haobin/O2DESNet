using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Demos.Workshop
{
    class Program
    {
        static void Main(string[] args)
        {            
            int seed = 0;

            var sim = new Simulator(new Status(Scenario.GetExample_PedrielliZhu2015(2, 5, 4, 3, 6))
            {
                Seed = seed,
                //Display = true,
                LogFile = string.Format("workshop_log_{0}.txt", seed),
            });

            //var sim = new Simulator(new Status(Scenario.GetExample_Xu2015(6, 5, 7, 9, 8))
            //{
            //    Seed = seed,
            //    Display = true,
            //    LogFile = string.Format("workshop_log_{0}.txt", seed),
            //});
            sim.Run(TimeSpan.FromDays(30));
        }
    }
}
