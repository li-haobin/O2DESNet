using O2DESNet.Warehouse.Statics;
using System;
using System.Linq;

namespace O2DESNet.Warehouse
{
    class Program
    {
        static void Main(string[] args)
        {
            //WarehouseSim whsim_base = new WarehouseSim("ZA");
            //var byteArray = Serializer.ObjectToByteArray(whsim_base);

            //foreach (PicklistGenerator.Strategy strategy in Enum.GetValues(typeof(PicklistGenerator.Strategy)))
            //{
            //    //WarehouseSim whsim = (WarehouseSim) Serializer.ByteArrayToObject(byteArray);
            //    //whsim.GeneratePicklist(strategy);

            //    WarehouseSim whsim = new WarehouseSim("ZA", strategy);
            //    whsim.Run(24);
            //    whsim.PrintStatistics();
            //}

            WarehouseSim whsim = new WarehouseSim("ZA", PicklistGenerator.Strategy.A);
            whsim.Run(24);
            whsim.PrintStatistics();

            whsim = new WarehouseSim("ZA", PicklistGenerator.Strategy.B);
            whsim.Run(24);
            whsim.PrintStatistics();

            whsim = new WarehouseSim("ZA", PicklistGenerator.Strategy.C);
            whsim.Run(24);
            whsim.PrintStatistics();

            whsim = new WarehouseSim("ZA", PicklistGenerator.Strategy.D);
            whsim.Run(24);
            whsim.PrintStatistics();

            Console.WriteLine("\n:: Experiment End ::");
            Console.ReadKey();
        }

        /// <summary>
        /// For debugging purposes
        /// </summary>
        /// <param name="pm"></param>

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

        static void DisplayPathingTable(Scenario pm)
        {
            foreach (var cp in pm.ControlPoints)
            {
                Console.WriteLine("Pathing Table at CP_{0}:", cp.Id);
                foreach (var item in cp.PathingTable)
                    Console.WriteLine("{0}:{1}", item.Key.Id, item.Value.Id);
                Console.WriteLine();
            }
        }
    }
}
