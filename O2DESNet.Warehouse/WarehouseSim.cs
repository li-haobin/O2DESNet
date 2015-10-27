using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse
{
    class WarehouseSim
    {
        public Simulator sim { get; private set; }
        public Scenario wh { get; private set; }

        public WarehouseSim(string scenarioName)
        {
            Initialize(scenarioName);
        }

        private void Initialize(string scenarioName)
        {
            wh = new Scenario(scenarioName);

            wh.ReadLayoutFiles();

            //var a1 = wh.CreateAisle("A1", 100);
            //var a2 = wh.CreateAisle("A2", 100);
            //var r1 = wh.CreateRow("R1", 20, a1, 0, a2, 0);
            //var r2 = wh.CreateRow("R2", 20, a1, 50, a2, 50);
            //var r3 = wh.CreateRow("R3", 20, a1, 100, a2, 100);

            //var r1s1 = wh.CreateShelf("R1S1", 5, r1, 10);
            //var r1s1k1 = wh.CreateRack("R1S1K1", r1s1, 3);

            //wh.AddToRack(new SKU("SKU0001", "Item 1"), r1s1k1);

            wh.Initialize();
            sim = new Simulator(wh, 0);
        }

        public void Run()
        {
            //while (true)
            //{
            //    sim.Run(10000);
            //    Console.Clear();
            //    foreach (var item in sim.Status.VehicleCounters)
            //        Console.WriteLine("CP{0}\t{1}", item.Key.Id, item.Value.TotalIncrementCount / (sim.ClockTime - DateTime.MinValue).TotalHours);
            //    Console.ReadKey();
            //}
        }
    }
}
