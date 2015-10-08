using System;

namespace O2DESNet.Demos.Workshop
{
    class Program
    {
        static void Main(string[] args)
        {
            var sim = new Simulator(Scenario.GetExample(2, 5, 4, 3, 6), 0);
            sim.Run(TimeSpan.FromDays(30));
        }
    }
}
