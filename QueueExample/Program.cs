using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var sim = new Simulator(new Status(new Scenario()) { Display = true });
            while (sim.Run(1)) Console.ReadKey();
        }
    }
}
