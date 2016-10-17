using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace O2DESNet.Demos.PathMoverSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            var pmSys = new PathMoverSystem(new PathMoverSystem.Statics { PathMover = GetPM1() }, 0);
            
            //pmSys.PathMover.Graph().View();
            //return;

            var sim = new Simulator(pmSys);
            //sim.Status.Display = true;

            //sim.WarmUp(TimeSpan.FromHours(3));

            while (true)
            {
                sim.Run(1);

                Console.Clear();
                Console.WriteLine("-------------------------");
                Console.WriteLine(sim.ClockTime);
                sim.WriteToConsole();

                pmSys.PathMover.Graph(sim.ClockTime).View();
                Console.ReadKey();
            }


            var config = GetPM1();
            var pm = new PathMover(config, 0);
            var pm1 = new PathMover(config, 0);

        }

        static PathMover.Statics GetPM1()
        {
            var pm = new PathMover.Statics();
            var paths = Enumerable.Range(0, 6).Select(i => pm.CreatePath(length: 100, fullSpeed: 100,   
                capacity: 2,              
                //direction: Path.Direction.TwoWay
                direction: Path.Direction.Forward
                )).ToArray();
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

            paths[0].Coords = new List<Point> { new Point(0, 0), new Point(100, 0) };
            paths[1].Coords = new List<Point> { new Point(100, 0), new Point(100, 100) };
            paths[2].Coords = new List<Point> { new Point(100, 100), new Point(0, 100) };
            paths[3].Coords = new List<Point> { new Point(0, 100), new Point(0, 0) };
            paths[4].Coords = new List<Point> { new Point(50, 0), new Point(50, 100) };
            paths[5].Coords = new List<Point> { new Point(100, 50), new Point(0, 50) };
            return pm;
        }

        static void DisplayRouteTable(PathMover.Statics pm)
        {
            foreach (var cp in pm.ControlPoints)
            {
                Console.WriteLine("Route Table at CP_{0}:", cp.Index);
                foreach (var item in cp.RoutingTable)
                    Console.WriteLine("{0}:{1}", item.Key.Index, item.Value.Index);
                Console.WriteLine();
            }
        }
    }
    
}
