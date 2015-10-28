using O2DESNet.PathMover.Statics;
using System;
using System.Linq;

namespace O2DESNet.PathMover
{
    class Program
    {
        /// Next things to do: 
        /// 1. Collision detector / path speed limit can be removed, since target speed is explicitly controled?
        /// 2. AGV Control logic (given route, auto Collision avoidance by speed control)
        /// 3. What if vehicle sizes are considered (for point 1 & 2)
        /// 4. conflicting path / control points
        /// 
        /// 5. Test scenarios (AGV, O/D generation etc.)
        /// 6. with 2-layer zone concept
        /// 
        /// 7. integrate into warehouse simulator

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
            var vt1 = new VehicleType(maxSpeed: 20, maxAcceleration: 20, maxDeceleration: 20);
            var vt2 = new VehicleType(maxSpeed: 30, maxAcceleration: 15, maxDeceleration: 10);
            pm.AddVehicles(vt1, 2);
            pm.AddVehicles(vt2, 3);

            pm.Initialize();
            var sim = new Simulator(pm, 0);

            while (true)
            {
                sim.Run(TimeSpan.FromDays(1));
                Console.Clear();
                Console.WriteLine("# Pass-Overs: {0}", sim.Status.Count_PassOvers);
                Console.WriteLine("# Cross-Overs: {0}", sim.Status.Count_CrossOvers);
                foreach (var item in sim.Status.VehicleCounters)
                    Console.WriteLine("CP{0}\t{1}", item.Key.Id, item.Value.TotalIncrementCount / (sim.ClockTime - DateTime.MinValue).TotalHours);
                Console.ReadKey();
            }
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
