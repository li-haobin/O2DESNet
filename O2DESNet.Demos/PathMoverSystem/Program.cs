using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using O2DESNet.SVGRenderer;
using System.Xml.Linq;

namespace O2DESNet.Demos.PathMoverSystem
{
    class Program
    {
        static void Main(string[] args)
        {

            var pmSys = new PathMoverSystem(new PathMoverSystem.Statics { PathMover = GetPM1() }, 0);

            var sim = new Simulator(pmSys);
            //sim.Status.Display = true;
            
            sim.WarmUp(TimeSpan.FromSeconds(60) );
            while (true)
            {
                sim.Run(1);

                //Console.Clear();
                //Console.WriteLine(sim.ClockTime);
                //sim.WriteToConsole();
                //Console.WriteLine();

                if (sim.ClockTime > DateTime.MinValue.AddSeconds(60 * 1.5)) break;
                //Console.ReadKey();
            }

            var svg = new SVG(1200, 1200,
               pmSys.PathMover.SVGDefs,
               pmSys.PathMover.SVG(25, 25, 0)
               );

            svg.View();

            return;

        }
       
        static PathMover.Statics GetPM1()
        {
            var pm = new PathMover.Statics();
            var paths = Enumerable.Range(0, 6).Select(i => pm.CreatePath(length: 100, fullSpeed: 100,   
                capacity: 4,              
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

            paths[0].Description = "M 0 0 L 1000 0";
            paths[1].Description = "M 1000 0 L 900 250 L 1000 500 L 1100 750 L 1000 1000";
            paths[2].Description = "M 1000 1000 L 0 1000";
            paths[3].Description = "M 0 1000 L 0 0";
            paths[4].Description = "M 500 0 L 500 1000";
            paths[5].Description = "M 1000 500 L 900 510 L 800 490 L 700 510 L 600 490 L 500 500 L 400 510 L 300 490 L 200 510 L 100 490 L 0 500";
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
