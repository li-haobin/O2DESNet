using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var pm = new Grid(
                Enumerable.Repeat(200d, 6).ToArray(),
                new double[] { 200 }.Concat(Enumerable.Repeat(90d, 4)).ToArray(),
                10);
            pm.DrawToImage("grid.png", 2000, 2000);

            int nVehicles = 10;
            while (true)
            {
                var scenario = new Scenario(Enumerable.Repeat(200d, 4).ToArray(), Enumerable.Repeat(45d, 3).ToArray(), 10, nVehicles);
                var sim = new Simulator(new Status(scenario, 0));
                sim.Run(TimeSpan.FromHours(3));
                Console.WriteLine("{0}\t{1}", nVehicles, sim.Status.JobsCount);
                
                Console.WriteLine("\nPath Utilizations:\n===========================");
                foreach (var util in sim.Status.GridStatus.PathUtils)
                    Console.WriteLine("{0}\t{1}", util.Key, util.Value.AverageCount);
                Console.WriteLine("Total # of Jobs: {0}", sim.Status.JobsCount);

                Console.ReadKey();
                nVehicles += 10;
            }




            //var sim = new Simulator(new Status(scenario));
            //sim.Status.Display = true;

            //while (sim.Run(1)) Console.ReadKey();
            //sim.Run(TimeSpan.FromHours(3));
            
        }

        static PMStatics GetPM1()
        {
            var pm = new PMStatics();
            var paths = Enumerable.Range(0, 6).Select(i => pm.CreatePath(length: 100, fullSpeed: 10, direction: Direction.Forward)).ToArray();
            pm.Connect(paths[0], paths[1]);
            pm.Connect(paths[1], paths[2]);
            pm.Connect(paths[2], paths[3]);
            pm.Connect(paths[3], paths[0]);
            pm.Connect(paths[0], paths[4], 50, 0);
            pm.Connect(paths[2], paths[4], 50, 100);
            pm.Connect(paths[1], paths[5], 50, 0);
            pm.Connect(paths[3], paths[5], 50, 100);
            pm.Connect(paths[4], paths[5], 50, 50);
            var cp1 = pm.CreateControlPoint(paths[0], 30);
            var cp2 = pm.CreateControlPoint(paths[0], 40);

            //var cp3 = pm.CreateControlPoint(paths[2], 30);
            //var cp4 = pm.CreateControlPoint(paths[2], 40);
            return pm;
        }
        
        static void DisplayRouteTable(PMStatics pm)
        {
            foreach (var cp in pm.ControlPoints)
            {
                Console.WriteLine("Route Table at CP_{0}:", cp.Id);
                foreach (var item in cp.RoutingTable)
                    Console.WriteLine("{0}:{1}", item.Key.Id, item.Value.Id);
                Console.WriteLine();
            }
        }
    }
}
