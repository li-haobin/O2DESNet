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

            sim.WarmUp(TimeSpan.FromMinutes(1));
            var lastClockTime = sim.ClockTime;
            var lastPostures = pmSys.PathMover.Vehicles.ToDictionary(veh => veh, veh => veh.GetPosture(sim.ClockTime));
                                  
            while (true)
            {
                sim.Run(1);

                //Console.Clear();
                //Console.WriteLine("-------------------------");
                Console.WriteLine(sim.ClockTime);
                //sim.WriteToConsole();

                foreach (var veh in pmSys.PathMover.Vehicles)
                    veh.Postures.Add(new Tuple<DateTime, Tuple<Point, double>>(sim.ClockTime, veh.GetPosture(sim.ClockTime)));


                //pmSys.PathMover.Graph(sim.ClockTime).View();
                if (sim.ClockTime > DateTime.MinValue.AddMinutes(10)) break;
                //Console.ReadKey();
            }

            var svg = Motion(pmSys.PathMover, 5);
            svg.View();

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

        static SVG Motion(PathMover pathMover, double scale = 5)
        {
            var svg = pathMover.Config.Graph(scale);
            svg.Styles.AddRange(Vehicle.Statics.SVGStyles);
            
            var start = pathMover.Vehicles.Min(veh => veh.Postures.First().Item1);
            var last = pathMover.Vehicles.Min(veh => veh.Postures.Last().Item1);
            var totalSeconds = (last - start).TotalSeconds;
            var dur = string.Format("{0}s", totalSeconds);
                        
            foreach (var veh in pathMover.Vehicles)
            {
                string keyTimes = "", xValues = "", yValues = "", degreeValues = "";
                foreach (var record in veh.Postures)
                {
                    keyTimes += string.Format("{0};", keyTimes.Length == 0 ? 0 : Math.Min(1, (record.Item1 - start).TotalSeconds / totalSeconds));
                    xValues += string.Format("{0};", record.Item2.Item1.X * scale + svg.Reference.X);
                    yValues += string.Format("{0};", record.Item2.Item1.Y * scale + svg.Reference.Y);
                    degreeValues += string.Format("{0},9.875,4.175;", record.Item2.Item2);
                }
                keyTimes = keyTimes.Remove(keyTimes.Length - 1);
                xValues = xValues.Remove(xValues.Length - 1);
                yValues = yValues.Remove(yValues.Length - 1);
                degreeValues = degreeValues.Remove(degreeValues.Length - 1);

                var animateX = new Dictionary<string, string> { { "attributeName", "x" }, { "dur", dur }, { "repeatCount", "indefinite" }, { "values", xValues }, { "keyTimes", keyTimes } };
                var animateY = new Dictionary<string, string> { { "attributeName", "y" }, { "dur", dur }, { "repeatCount", "indefinite" }, { "values", yValues }, { "keyTimes", keyTimes } };
                var animateDegree = new Dictionary<string, string> { { "attributeName", "transform" }, { "type", "rotate" }, { "calcMode", "discrete" }, { "dur", dur }, { "repeatCount", "indefinite" }, { "values", degreeValues }, { "keyTimes", keyTimes } };

                keyTimes = "";
                var values = "";
                while (veh.StateHistory.First().Item1 < start) veh.StateHistory.RemoveAt(0);
                foreach (var record in veh.StateHistory)
                {
                    keyTimes += string.Format("{0};", keyTimes.Length == 0 ? 0 : Math.Max(0, Math.Min(1, (record.Item1 - start).TotalSeconds / totalSeconds)));
                    values += string.Format("{0};", record.Item2 == Vehicle.State.Parking ? "visible" : "hidden");
                }
                keyTimes += "1";
                values += "hidden";
                var animateParkingLabel = new Dictionary<string, string> { { "attributeName", "visibility" }, { "dur", dur }, { "repeatCount", "indefinite" }, { "values", values }, { "keyTimes", keyTimes } };

                svg.Body += "<defs><g id=\"vehCate_" + veh.Id + "\">\n" +
                    "<rect width=\"19.75\" height=\"8.35\" stroke=\"black\" fill=\"" + veh.Category.Color + "\" fill-opacity=\"0.5\" />\n" +
                    "<text class=\"vehCate_label\" transform=\"translate(9.875,8.175)\">" + veh.Category.Name + "</text>\n" +
                    "<text class=\"vehCate_sign\" transform=\"translate(9.875,20)\">\n" + GetXML("animate", animateParkingLabel) +
                    "PARKING</text>" +
                    GetXML("animateTransform", animateDegree) +
                    "</g></defs>\n" +
                    "<use transform=\"translate(-9.875,-4.175)\" href=\"#vehCate_" + veh.Id + "\">\n" + GetXML("animate", animateX) + GetXML("animate", animateY) + "</use>\n";
            }
            return svg;
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
