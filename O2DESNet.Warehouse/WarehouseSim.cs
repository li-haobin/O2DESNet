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
            sim = new Simulator(wh, 0);

            // wh.ReadLayoutFiles();

            BasicBuilder();
            wh.InitializeRouting(); // Probably need serialise... Since it's sort of constant. Just have to check if there is no change.

            wh.ReadSKUsFile();
            wh.ReadMasterPickList();
            
        }

        private void BasicBuilder()
        {
            // Dimensions in metres

            string[] zone = { "A", "B", "C", "D", "E", "F", "Y", "Z" }; // In pairs
            int numPairs = zone.Count() / 2;
            int numRows = 2; //160
            int numShelves = 2; //20
            int numRacks = 2; //6
            double interRowSpace = 1.7;
            double shelfWidth = 1.5;
            double rackHeight = 0.35;

            double aisleLength = numRows * interRowSpace;
            double rowLength = numShelves * shelfWidth;
            double shelfHeight = numRacks * rackHeight;
            double interAisleDist = 2 * rowLength;

            var mainAisle = wh.CreateAisle("MAIN", numPairs * interAisleDist);
            wh.StartCP = wh.CreateControlPoint(mainAisle, 0);

            // rowPair
            for (int z = 0; z < numPairs; z++)
            {
                var pairAisle = wh.CreateAisle(zone[2 * z] + zone[2 * z + 1], aisleLength);

                wh.Connect(mainAisle, pairAisle, (z + 1) * interAisleDist, 0);

                // Rows
                for (int i = 1; i <= numRows; i++)
                {
                    var row1 = wh.CreateRow(zone[2 * z] + "-" + i.ToString(), rowLength, pairAisle, i * interRowSpace);
                    var row2 = wh.CreateRow(zone[2 * z + 1] + "-" + i.ToString(), rowLength, pairAisle, i * interRowSpace);

                    // Shelves
                    for (int j = 1; j <= numShelves; j++)
                    {
                        var shelf1 = wh.CreateShelf(row1.Row_ID + "-" + j.ToString(), shelfHeight, row1, j * shelfWidth);
                        var shelf2 = wh.CreateShelf(row2.Row_ID + "-" + j.ToString(), shelfHeight, row2, j * shelfWidth);

                        // Racks
                        for (int k = 1; k <= numRacks; k++)
                        {
                            wh.CreateRack(shelf1.Shelf_ID + "-" + k.ToString(), shelf1, k * rackHeight);
                            wh.CreateRack(shelf2.Shelf_ID + "-" + k.ToString(), shelf2, k * rackHeight);
                        }
                    }
                }
            }
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
