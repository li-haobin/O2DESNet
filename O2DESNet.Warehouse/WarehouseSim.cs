using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using O2DESNet.Warehouse.Statics;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse
{
    [Serializable]
    class WarehouseSim
    {
        public Simulator sim { get; private set; }
        public Scenario wh { get; private set; }
        public PicklistGenerator.Strategy? strategy { get; set; }
        public PicklistGenerator generator { get; set; }

        public WarehouseSim(string scenarioName, PicklistGenerator.Strategy? strategy = null)
        {
            this.strategy = strategy;
            InitializeScenario(scenarioName);
            sim = new Simulator(wh); // Only after warehouse has been built and initialised properly.
        }

        public void GeneratePicklist(PicklistGenerator.Strategy strategy)
        {
            this.strategy = strategy;
            generator.Generate(strategy, wh, true);
        }

        private void InitializeScenario(string scenarioName)
        {
            wh = new Scenario(scenarioName);
            generator = new PicklistGenerator();

            // Generate layout //
            //wh.ReadLayoutFiles();
            //BasicBuilder(wh);
            LayoutBuilder.ZABuilderEila(wh);


            wh.ReadSKUsFile();
            wh.ReadPickers();

            // Only call after SKU and pickers
            generator.ReadOrders(wh, "ZA_Orders.csv");
            if (strategy != null) generator.Generate((PicklistGenerator.Strategy)strategy, wh, true);
            //wh.ReadMasterPickList(); // Possible to get directly from PicklistGenerator

            wh.InitializeRouting();
        }

        // TODO: Build another layout based on Elia files
        private void BasicBuilder(Scenario scenario)
        {
            // Dimensions in metres

            string[] zone = { "Y", "Z", "A", "B", "C", "D", "E", "F" }; // In pairs
            int numPairs = zone.Count() / 2;
            int numRows = 160; //160
            int numShelves = 20; //20
            int numRacks = 9; //9
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
            Console.WriteLine("*********************************************************");
            //Console.WriteLine("Number of orders = {0}", PicklistGenerator.AllOrders.Count);

            if (strategy == PicklistGenerator.Strategy.A)
            {
                Console.WriteLine(":: Strategy A ::\n");
                PrintTypeStatistics(sim.Scenario.GetPickerType[PicklistGenerator.A_PickerID]);
            }
            if (strategy == PicklistGenerator.Strategy.B)
            {
                Console.WriteLine(":: Strategy B ::\n");
                PrintTypeStatistics(sim.Scenario.GetPickerType[PicklistGenerator.B_PickerID_SingleZone]);
                PrintTypeStatistics(sim.Scenario.GetPickerType[PicklistGenerator.B_PickerID_MultiZone]);
                PrintTypeStatistics(sim.Scenario.GetPickerType[PicklistGenerator.B_PickerID_SingleItem]);
            }
            if (strategy == PicklistGenerator.Strategy.C)
            {
                Console.WriteLine(":: Strategy C ::\n");
                PrintTypeStatistics(sim.Scenario.GetPickerType[PicklistGenerator.C_PickerID_SingleZone]);
                PrintTypeStatistics(sim.Scenario.GetPickerType[PicklistGenerator.C_PickerID_MultiZone]);
                PrintTypeStatistics(sim.Scenario.GetPickerType[PicklistGenerator.C_PickerID_SingleItem]);
            }
            if (strategy == PicklistGenerator.Strategy.D)
            {
                Console.WriteLine(":: Strategy D ::\n");
                PrintTypeStatistics(sim.Scenario.GetPickerType[PicklistGenerator.D_PickerID_MultiItem]);
                PrintTypeStatistics(sim.Scenario.GetPickerType[PicklistGenerator.D_PickerID_SingleItem]);
            }

            Console.WriteLine("");
            Console.WriteLine("Ave / Max Active Pickers: {0} / {1}", sim.Status.GetAverageNumActivePickers(), sim.Status.MaxActivePickers);

            if (strategy == PicklistGenerator.Strategy.C || strategy == PicklistGenerator.Strategy.D)
            {
                // Order Batching in Pure Zone for Consolidation
                Console.WriteLine("");
                Console.WriteLine("Ave / Max Number of Items per Order Batch: {0:0.00} / {1}", sim.Status.GetAverageNumItemsSorted(), sim.Status.MaxNumItemsSorted);
                Console.WriteLine("Ave / Max Number of Totes per Order Batch: {0:0.00} / {1}", sim.Status.GetAverageOrderBatchesTotesCount(), sim.Status.GetMaxOrderBatchesTotesCount());
                Console.WriteLine("Number of Order Batches: {0}", OrderBatch.GetTotalNumBatches());
                Console.WriteLine("Number of Sorting Stations Utilised: {0}", sim.Status.GetNumSortingStations());
                Console.WriteLine("");
            }



            Console.WriteLine("Simulation run length: {0:hh\\:mm\\:ss}", sim.ClockTime - DateTime.MinValue);
            Console.WriteLine("*********************************************************\n");
        }

        private void PrintTypeStatistics(PickerType type)
        {
            int totalPickList = sim.Status.TotalPickListsCompleted[type];
            int totalPickJob = sim.Status.TotalPickJobsCompleted[type];
            double averateUtil;

            if (type.PickerType_ID == PicklistGenerator.A_PickerID ||
                type.PickerType_ID == PicklistGenerator.B_PickerID_SingleZone ||
                type.PickerType_ID == PicklistGenerator.B_PickerID_MultiZone ||
                type.PickerType_ID == PicklistGenerator.C_PickerID_SingleZone)
            {
                // Order-based
                averateUtil = 1.0 * generator.NumOrders[type] / totalPickList;
            }
            else
            {
                // Item-based
                averateUtil = 1.0 * totalPickJob / totalPickList;
            }

            Console.WriteLine("-- For PickerType {0}, {1,2} pickers --", type.PickerType_ID, sim.Scenario.NumPickers[type]);
            Console.WriteLine("Number of orders: {0}", generator.NumOrders[type]);
            Console.WriteLine("Number of carts: {0}", sim.Scenario.NumPickers[type]);
            Console.WriteLine("Total Picklists Completed: {0}", totalPickList);
            Console.WriteLine("Total Pickjobs (items) Completed: {0}", totalPickJob);
            Console.WriteLine("Average Cart Utilisation: {0:0.00} ({1:P})", averateUtil, averateUtil / type.Capacity);
            Console.WriteLine("Average Items per Cart: {0:0.00}", 1.0 * totalPickJob / totalPickList);
            Console.WriteLine("Average PickList Completion Time: {0:hh\\:mm\\:ss}", sim.Status.GetAveragePickListTime(type));
            Console.WriteLine("-------------------------------------");
        }

    }
}
