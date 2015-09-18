using O2DESNet.PathMover;
using System;
using System.Linq;

namespace PathMover_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var pm = new O2DESNet.PathMover.System();
            var paths = Enumerable.Range(0,6).Select(i=> pm.CreatePath(100, 1, Direction.Forward)).ToArray();
            pm.Connect(paths[0], paths[1]);
            pm.Connect(paths[1], paths[2]);
            pm.Connect(paths[2], paths[3]);
            pm.Connect(paths[3], paths[0]);
            pm.Connect(paths[0], paths[4], 50, 0);
            pm.Connect(paths[2], paths[4], 50, 100);
            pm.Connect(paths[1], paths[5], 50, 0);
            pm.Connect(paths[3], paths[5], 50, 100);
            pm.Connect(paths[4], paths[5], 50, 50);

            pm.CreateControlPoint(paths[0], 30);

            pm.ConstructRouteTables();
            //DisplayRouteTable(pm);

        }

        static void DisplayRouteTable(O2DESNet.PathMover.System pm)
        {
            foreach (var cp in pm.ControlPoints)
            {
                Console.WriteLine("Route Table at CP_{0}:", cp.Id);
                foreach (var item in cp.RouteTable)
                    Console.WriteLine("{0}:{1}", item.Key.Id, item.Value.Id);
                Console.WriteLine();
            }
        }
    }
}
