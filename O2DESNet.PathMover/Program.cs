using O2DESNet.PathMover.Statics;
using System;
using System.Linq;

namespace O2DESNet.PathMover
{
    class Program
    {
        static void Main(string[] args)
        {
            var pm = new Scenario();
            var paths = Enumerable.Range(0, 6).Select(i => pm.CreatePath(length: 100, maxSpeed: 20, direction: Direction.Forward)).ToArray();
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
            var vt1 = new VehicleType(maxSpeed: 14, maxAcceleration: 20, maxDeceleration: 20);
            var vt2 = new VehicleType(maxSpeed: 30, maxAcceleration: 15, maxDeceleration: 10);
            pm.AddVehicles(vt1, 2);
            pm.AddVehicles(vt2, 3);

            var sim = new Simulator(pm, 0);

            //pm.Initialize();
            //double toSpeed = 12, peakSpeed;
            //var time = vt.GetShortestTravelingTime(cp1, cp2, 0, ref toSpeed, out peakSpeed);
            
            //DisplayRouteTable(pm);
            Console.WriteLine();
            

        }

        static void DisplayRouteTable(Scenario pm)
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
