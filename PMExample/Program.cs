using O2DESNet.PathMover;
using PMExample.Statics;
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
            var pm = new Grid(Enumerable.Repeat(200d, 6).ToArray(), Enumerable.Repeat(45d, 8).ToArray(), 10);
            var quayPoints = pm.RowPaths[0].Select(p => pm.CreateControlPoint(p, p.Length / 2)).ToArray();
            var yardPoints = Enumerable.Range(1, pm.RowPaths.Length - 1).SelectMany(i => pm.RowPaths[i].Select(p => pm.CreateControlPoint(p, p.Length / 2))).ToArray();

            var sim = new Simulator(new Status(new Scenario { PM = pm, JobHourlyRate = 1000 }));
            //sim.Status.Display = true;

            //while (sim.Run(1)) Console.ReadKey();
            sim.Run(TimeSpan.FromDays(1));

            Console.WriteLine("\nPath Utilizations:\n===========================");
            foreach (var util in sim.Status.PM.PathUtils)
                Console.WriteLine("{0}\t{1}", util.Key, util.Value.AverageCount);
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
        
        //static PMStatics GetYardTraffic()
        //{
        //    var pm = new PMStatics();
        //    int M = 2 * 3; //3 is the number of berth 
        //    int N = 8; //number of the block  //N >= 2
        //    var paths_hori = Enumerable.Range(0, 2 * (N + 1) * M).Select(i => pm.CreatePath(length: 200, fullSpeed: 10, direction: Direction.Forward)).ToArray();
        //    var paths_vert = Enumerable.Range(0, 2 * N * (M + 1)).Select(i => pm.CreatePath(length: 45, fullSpeed: 10, direction: Direction.Forward)).ToArray();

        //    #region CPs
        //    for (int i = 0; i < N; i++)
        //        for (int j = 0; j < M; j++)
        //        {
        //            pm.CreateControlPoint(path: paths_hori[M * 2 * i + j], position: 0);
        //            pm.CreateControlPoint(path: paths_hori[M * 2 * i + M - 1], position: paths_hori[M * 2 * i + M - 1].Length);
        //        }
        //    for (int i = 0; i < N; i++)
        //        for (int j = 0; j < M; j++)
        //        {
        //            //additional control points in the middle of the horizontal paths
        //            pm.CreateControlPoint(path: paths_hori[M * i + j], position: 100);
        //        }
        //    var cp = pm.ControlPoints;
        //    #endregion

        //    #region Paths
        //    //Create Paths
        //    for (int i = 0; i < N; i++)
        //    {
        //        for (int j = 0; j < M; j++)
        //        {
        //            pm.Connect(paths_vert[2 * N * j + i], paths_hori[2 * M * i + j], 45, 0, cp[2 * M * i + j]);
        //            pm.Connect(paths_hori[2 * M * i + j], paths_vert[2 * N * j + i + 3 * N], 200, 0, cp[2 * M * i + j + 1]);
        //            pm.Connect(paths_vert[2 * N * j + i + 3 * N], paths_hori[2 * M * i + j + 3 * M], 45, 0, cp[2 * M * i + j + M + 2]);
        //            pm.Connect(paths_hori[2 * M * i + j + 3 * M], paths_vert[2 * N * j + i], 200, 0, cp[2 * M * i + j + M + 1]);

        //            pm.Connect(paths_vert[(2 * j + 1) * N + i], paths_hori[(2 * i + 2) * M + j], 45, 0, cp[2 * M * i + j + M + 1]);
        //            pm.Connect(paths_hori[(2 * i + 2) * M + j], paths_vert[(2 * j + 2) * N + i], 200, 0, cp[2 * M * i + j + M + 2]);
        //            pm.Connect(paths_vert[(2 * j + 2) * N + i], paths_hori[(2 * i + 1) * M + j], 45, 0, cp[2 * M * i + j + 1]);
        //            pm.Connect(paths_hori[M * 2 * j + i + M], paths_vert[(2 * j + 1) * N + i], 200, 0, cp[2 * M * i + j]);

        //        }
        //    }
        //    #endregion
            
        //    return pm;
        //}

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
