using O2DESNet.PathMover;
using System;
using System.Linq;

namespace PathMoverTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var pm = new O2DESNet.PathMover.System();
            var paths = Enumerable.Range(0, 6).Select(i => pm.CreatePath(100, 20, Direction.Forward)).ToArray();
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
            var vt = pm.CreateVehicleType(14, 20, 20);
            
            pm.Initialize();
            double toSpeed = 12, peakSpeed;
            var time = vt.GetShortestTravelingTime(cp1, cp2, 0, ref toSpeed, out peakSpeed);


            //DisplayRouteTable(pm);
            Console.WriteLine();
            

        }

        static void DisplayRouteTable(O2DESNet.PathMover.System pm)
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
