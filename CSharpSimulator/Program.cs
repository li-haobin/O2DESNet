using CSharpSimulator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            var simulations = new Simulation[]{
                new Simulation(Cluster.GetCluster1(), 0),
                new Simulation(Cluster.GetCluster1(), 1),
                new Simulation(Cluster.GetCluster1(), 2),
            };
            

            while (true)
            {
                Console.Write("Input Number of Events to Run: ");
                int nEvents = Convert.ToInt32(Console.ReadLine());
                if (nEvents > 0)
                {
                    Simulation.Run_withTimeDilation(simulations, nEvents);
                    Console.WriteLine();
                }
                else if (nEvents == 0)
                {
                    foreach (var s in simulations) s.TimeDilationScale = Convert.ToDouble(Console.ReadLine());
                }
                else break;               

                //simulation1.Run(nEvents);
                //foreach (var q in simulation.Queues) Console.WriteLine(q.Count);
            }            
        }
    }
}
