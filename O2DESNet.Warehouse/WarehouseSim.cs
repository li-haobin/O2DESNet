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
            InitializeScenario(scenarioName);
            sim = new Simulator(wh, 0); // Only after warehouse has been built and initialised properly.
        }

        private void InitializeScenario(string scenarioName)
        {
            wh = new Scenario(scenarioName);

            // Generate layout
            // wh.ReadLayoutFiles();
            BasicBuilder(wh);

            // Init layout, SKU and pickers
            wh.InitializeRouting(); // Probably need serialise... Since it's sort of constant. Just have to check if there is no change.
            wh.ReadSKUsFile();
            wh.ReadPickers();

            // Only call after initialisation of layout, SKU and pickers
            // PicklistGenerator.Generate(PicklistGenerator.Strategy.A, wh);
            wh.ReadMasterPickList(); // Possible to get directly from PicklistGenerator ??
        }

        private void BasicBuilder(Scenario scenario)
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

            var mainAisle = scenario.CreateAisle("MAIN", numPairs * interAisleDist);
            scenario.StartCP = scenario.CreateControlPoint(mainAisle, 0);

            // rowPair
            for (int z = 0; z < numPairs; z++)
            {
                var pairAisle = scenario.CreateAisle(zone[2 * z] + zone[2 * z + 1], aisleLength);

                scenario.Connect(mainAisle, pairAisle, (z + 1) * interAisleDist, 0);

                // Rows
                for (int i = 1; i <= numRows; i++)
                {
                    var row1 = scenario.CreateRow(zone[2 * z] + "-" + i.ToString(), rowLength, pairAisle, i * interRowSpace);
                    var row2 = scenario.CreateRow(zone[2 * z + 1] + "-" + i.ToString(), rowLength, pairAisle, i * interRowSpace);

                    // Shelves
                    for (int j = 1; j <= numShelves; j++)
                    {
                        var shelf1 = scenario.CreateShelf(row1.Row_ID + "-" + j.ToString(), shelfHeight, row1, j * shelfWidth);
                        var shelf2 = scenario.CreateShelf(row2.Row_ID + "-" + j.ToString(), shelfHeight, row2, j * shelfWidth);

                        // Racks
                        for (int k = 1; k <= numRacks; k++)
                        {
                            scenario.CreateRack(shelf1.Shelf_ID + "-" + k.ToString(), shelf1, k * rackHeight);
                            scenario.CreateRack(shelf2.Shelf_ID + "-" + k.ToString(), shelf2, k * rackHeight);
                        }
                    }
                }
            }
        }

        public void Run(double hours)
        {
            sim.Run(TimeSpan.FromHours(hours));
        }

        public void PrintStatistics()
        {
            foreach (var type in sim.Scenario.NumPickers)
            {
                Console.WriteLine("-- For PickerType {0}, {1} pickers --", type.Key.PickerType_ID, sim.Scenario.NumPickers[type.Key]);
                Console.WriteLine("Total Picklists Completed: {0}", sim.Status.TotalPickListsCompleted[type.Key]);
                Console.WriteLine("Total Pickjobs Completed: {0}", sim.Status.TotalPickJobsCompleted[type.Key]);
                Console.WriteLine("Average Picking Time: {0}", sim.Status.GetAveragePickListTime(type.Key));
                Console.WriteLine("-------------------------------------");
            }
        }
    }
}
