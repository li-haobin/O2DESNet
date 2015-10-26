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
        public Simulator sim { get; set; }
        public Scenario pm { get; set; }

        public WarehouseSim()
        {
            Initialize();
        }

        private void Initialize()
        {
            pm = new Scenario();
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
            var vt1 = new PickerType(maxSpeed: 14, maxAcceleration: 20, maxDeceleration: 20);
            var vt2 = new PickerType(maxSpeed: 30, maxAcceleration: 15, maxDeceleration: 10);
            pm.AddPickers(vt1, 2);
            pm.AddPickers(vt2, 3);

            pm.Initialize();
            sim = new Simulator(pm, 0);
        }

        public void Run()
        {
            while (true)
            {
                sim.Run(10000);
                Console.Clear();
                foreach (var item in sim.Status.VehicleCounters)
                    Console.WriteLine("CP{0}\t{1}", item.Key.Id, item.Value.TotalIncrementCount / (sim.ClockTime - DateTime.MinValue).TotalHours);
                Console.ReadKey();
            }
        }
    }
}
