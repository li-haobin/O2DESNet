using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using O2DESNet.SVGRenderer;

namespace O2DESNet.Demos.PathMoverSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            var pmSys = new PathMoverSystem(new PathMoverSystem.Statics { PathMover = GetPM1() }, 0);

            new SVGRenderer.SVG(1050, 1050,
                PathMover.Statics.SVGDefs,
                pmSys.PathMover.Config.SVG(25, 25, 0),
                new Group("veh", x:300, y:300, rotate:30, content: new Vehicle.Statics().SVG())
                ).View();

            return;
            
            //pmSys.PathMover.Graph().View();
            //return;

            var sim = new Simulator(pmSys);
            //sim.Status.Display = true;
            
                                  
            while (true)
            {
                sim.Run(1);

                //Console.Clear();
                //Console.WriteLine("-------------------------");
                Console.WriteLine(sim.ClockTime);
                //sim.WriteToConsole();
                

                //pmSys.PathMover.Graph(sim.ClockTime).View();
                if (sim.ClockTime > DateTime.MinValue.AddMinutes(2)) break;
                //Console.ReadKey();
            }
            

            //while (true)
            //{
            //    sim.Run(1);

            //    Console.Clear();
            //    Console.WriteLine("-------------------------");
            //    Console.WriteLine(sim.ClockTime);
            //    sim.WriteToConsole();

            //    pmSys.PathMover.Graph(sim.ClockTime).View();
            //    Console.ReadKey();
            //}         


            var config = GetPM1();
            var pm = new PathMover(config, 0);
            var pm1 = new PathMover(config, 0);

        }
       
        static string GetXML(string name, Dictionary<string,string> attributes)
        {
            string str = string.Format("<{0} ", name);
            foreach (var i in attributes) str += string.Format("{0}=\"{1}\" ", i.Key, i.Value);
            return str + "/>\n";
        }

        static PathMover.Statics GetPM1()
        {
            var pm = new PathMover.Statics();
            var paths = Enumerable.Range(0, 6).Select(i => pm.CreatePath(length: 100, fullSpeed: 100,   
                capacity: 3,              
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
            paths[1].Description = "M 1000 0 L 1000 1000";
            paths[2].Description = "M 1000 1000 L 0 1000";
            paths[3].Description = "M 0 1000 L 0 0";
            paths[4].Description = "M 500 0 L 500 1000";
            paths[5].Description = "M 1000 500 L 0 500";
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
