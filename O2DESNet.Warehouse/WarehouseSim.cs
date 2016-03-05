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
    public class WarehouseSim
    {
        public Simulator sim { get; private set; }
        public Scenario wh { get; private set; }
        public PicklistGenerator generator { get; set; }

        public PicklistGenerator.Strategy? strategy { get; set; }
        public int RunID { get; private set; }


        public WarehouseSim(string scenarioName, PicklistGenerator.Strategy? strategy = null, int runID = 1)
        {
            RunID = runID;
            this.strategy = strategy;
            IOHelper.ReadInputParams(scenarioName, RunID);
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
                Console.WriteLine("Number of Order Batches Created: {0}", OrderBatch.GetTotalNumBatches());
                Console.WriteLine("Sorting Buffer Size: {0}", sim.Status.GetMaxNumSortingStations());
                Console.WriteLine("Max Active Sorting Stations: {0}", sim.Status.MaxActiveSorters);
                Console.WriteLine("");
            }



            Console.WriteLine("Simulation run length: {0:hh\\:mm\\:ss}", sim.ClockTime - DateTime.MinValue);
            Console.WriteLine("*********************************************************\n");
        }

        private void PrintTypeStatistics(PickerType type)
        {
            int totalPickList = sim.Status.TotalPickListsCompleted[type];
            int totalPickJob = sim.Status.TotalPickJobsCompleted[type];
            double averageUtil;

            if (type.PickerType_ID == PicklistGenerator.A_PickerID ||
                type.PickerType_ID == PicklistGenerator.B_PickerID_SingleZone ||
                type.PickerType_ID == PicklistGenerator.B_PickerID_MultiZone ||
                type.PickerType_ID == PicklistGenerator.C_PickerID_SingleZone)
            {
                // Order-based
                averageUtil = 1.0 * generator.NumOrders[type] / totalPickList;
            }
            else
            {
                // Item-based
                averageUtil = 1.0 * totalPickJob / totalPickList;
            }

            Console.WriteLine("-- For PickerType {0}, {1,2} pickers --", type.PickerType_ID, sim.Scenario.NumPickers[type]);
            Console.WriteLine("Number of orders: {0}", generator.NumOrders[type]);
            Console.WriteLine("Number of carts: {0}", sim.Scenario.NumPickers[type]);
            Console.WriteLine("Total Picking Trips Completed: {0}", totalPickList);
            Console.WriteLine("Total Pickjobs (items) Completed: {0}", totalPickJob);
            Console.WriteLine("Average Cart Utilisation: {0:0.00} ({1:P})", averageUtil, averageUtil / type.Capacity);
            Console.WriteLine("Average Items per Cart: {0:0.00}", 1.0 * totalPickJob / totalPickList);
            Console.WriteLine("Min / Max Items per Cart: {0} / {1}", sim.Status.MinPickListSize[type], sim.Status.MaxPickListSize[type]);
            Console.WriteLine("Average PickList Completion Time: {0:hh\\:mm\\:ss}", sim.Status.GetAveragePickListTime(type));
            Console.WriteLine("-------------------------------------");
        }

        public void OutputRacks()
        {
            using (StreamWriter sw = new StreamWriter(@"Layout\" + wh.Name + "_PhysicalLayout.csv"))
            {
                foreach (var rack in wh.Racks.Keys)
                {
                    sw.WriteLine(rack);
                }
            }
        }

        internal List<string> GetOutputStatistics()
        {
            List<string> data = new List<string>();
            // include strategy name first row
            data.Add(strategy.Value.ToString("F")); // [0] Strategy name

            data.Add(GetAggregateTotalCycleTime().ToString()); // [1a] Total Cycle time, including consolidation and sorting
            data.Add(GetAggregatePickingCycleTime().ToString()); // [1b] Picking Cycle time (sec)

            if (strategy == PicklistGenerator.Strategy.C || strategy == PicklistGenerator.Strategy.D)
            {
                data.Add(GetAverageOrderBatchCompletionTime().ToString());// [2] Batch completion time (min)
            }
            else
            {
                data.Add("XX");
            }

            data.Add(GetToteThroughput().ToString()); // [3a] Tote throughput (totes/hour)
            data.Add(GetCartThroughput().ToString()); // [3b] Cart throughput (carts/hour)
            data.Add(OrderBatch.GetTotalNumBatches().ToString()); // [4] Number of batches issued

            data.Add(GetAllMinToteUtilisation().ToString()); // [5a] Min tote utilisation
            data.Add(GetAggregateAveToteUtilisation().ToString()); // [6a] Ave tote utilisation
            data.Add(GetAllMaxToteUtilisation().ToString()); // [7a] Max tote utilisation

            data.Add(GetAllMinCartUtilisation().ToString()); // [5b] Min tote utilisation
            data.Add(GetAggregateAveCartUtilisation().ToString()); // [6b] Ave tote utilisation
            data.Add(GetAllMaxCartUtilisation().ToString()); // [7b] Max tote utilisation

            if (strategy == PicklistGenerator.Strategy.C || strategy == PicklistGenerator.Strategy.D)
            {
                data.Add(sim.Status.GetMinOrderBatchesTotesCount().ToString()); // [8] Min number of totes per batch
                data.Add(sim.Status.GetAverageOrderBatchesTotesCount().ToString()); // [9] Ave number of totes per batch
                data.Add(sim.Status.GetMaxOrderBatchesTotesCount().ToString()); // [10] Max number of totes per batch
            }
            else
            {
                data.Add("XX");
                data.Add("XX");
                data.Add("XX");
            }

            data.Add(GetAllMinItemsPerTote().ToString()); // [11a] Min items per tote
            data.Add(GetAggregateAveItemsPerTote().ToString()); // [12a] Ave items per tote
            data.Add(GetAllMaxItemsPerTote().ToString()); // [13a] Max items per tote

            data.Add(GetAllMinItemsPerCart().ToString()); // [11b] Min items per cart
            data.Add(GetAggregateAveItemsPerCart().ToString()); // [12b] Ave items per cart
            data.Add(GetAllMaxItemsPerCart().ToString()); // [13b] Max items per cart

            data.Add(sim.Status.GetAverageNumActivePickers().ToString()); // [14] Ave number of active pickers
            data.Add(sim.Status.MaxActivePickers.ToString()); // [15] Max number of active pickers

            data.Add(GetAveragePickListWaitingTime().ToString()); // [16] Average waiting time for picking (min)

            if (strategy == PicklistGenerator.Strategy.C || strategy == PicklistGenerator.Strategy.D)
            {
                data.Add(GetOrderBatchAverageWaitingTime().ToString()); // [17] Average waiting time for batch consolidation (min)
                data.Add(sim.Status.GetAverageNumActiveSorters().ToString()); // [18] Ave number of sorting stations open
                data.Add(sim.Status.MaxActiveSorters.ToString()); // [19] Max number of sorting stations open

                data.Add(sim.Status.GetAverageNumBatchWaiting().ToString()); // [20] Ave number of batches in front of sorting stations
                data.Add(sim.Status.GetMaxNumSortingStations().ToString()); // [21] Max number of batches in front of sorting stations

                data.Add(sim.Status.GetAverageNumToteWaiting().ToString()); // [22] Ave number of totes in front of sorting stations
                data.Add(sim.Status.MaxTotesWaitingForSorting.ToString()); // [23] Max number of totes in front of sorting stations
            }
            else
            {
                data.Add("XX");
                data.Add("XX");
                data.Add("XX");

                data.Add("XX");
                data.Add("XX");

                data.Add("XX");
                data.Add("XX");
            }

            // TODO: number of orders to sorting
            data.Add(GetNumOrdersToSorting().ToString()); // [24] Number of orders to sorting
            // TODO: number of orders without sorting
            data.Add(GetNumOrdersWithoutSorting().ToString());// [25] Number of orders without sorting

            return data;
        }

        /// <summary>
        /// Aggregate cycle time for current strategy in seconds per item
        /// </summary>
        /// <returns>double cycle time</returns>
        internal double GetAggregatePickingCycleTime()
        {
            TimeSpan totalTime = TimeSpan.Zero;
            int totalJobs = 0;
            foreach (var PickerTypeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                var type = sim.Scenario.GetPickerType[PickerTypeID];
                totalTime += sim.Status.TotalPickingTime[type];
                totalJobs += sim.Status.TotalPickJobsCompleted[type];
            }

            return 1.0 * totalTime.TotalSeconds / totalJobs;
        }
        internal double GetAggregateTotalCycleTime() // Include average sorting time
        {
            TimeSpan totalTime = TimeSpan.Zero;
            int totalJobs = 0;
            foreach (var PickerTypeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                var type = sim.Scenario.GetPickerType[PickerTypeID];
                totalTime += sim.Status.TotalPickingTime[type];
                totalJobs += sim.Status.TotalPickJobsCompleted[type];

                // Sorting required
                if (PickerTypeID == PicklistGenerator.C_PickerID_MultiZone ||
                    PickerTypeID == PicklistGenerator.D_PickerID_MultiItem)
                {
                    // Time waiting for consolidation
                    foreach (var orderBatch in sim.Status.OrderBatchCompletionTime.Keys)
                    {
                        foreach (var picklist in orderBatch.PickLists)
                        {
                            totalTime += (orderBatch.CompletionTime - picklist.endPickTime).Multiply(picklist.ItemsCount);
                        }
                    }
                    // Sorting time
                    totalTime += TimeSpan.FromSeconds(wh.Consolidator.sortingRate * sim.Status.TotalPickJobsCompleted[type]);
                }
            }
            return 1.0 * totalTime.TotalSeconds / totalJobs;
        }

        internal double GetAverageOrderBatchCompletionTime()
        {
            var completionTimes = sim.Status.OrderBatchCompletionTime.Values.ToList();

            return completionTimes.Average(waitingTime => waitingTime.TotalMinutes);
        }
        internal double GetOrderBatchAverageWaitingTime()
        {
            var allWaitingTime = sim.Status.OrderBatchWaitingTimeForSorting.Values.ToList();

            return allWaitingTime.Average(waitingTime => waitingTime.TotalMinutes);
        }
        internal double GetAveragePickListWaitingTime()
        {
            int totalPickList = 0;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                var type = sim.Scenario.GetPickerType[typeID];
                totalPickList += sim.Status.TotalPickListsCompleted[type];
            }

            return 1.0 * sim.Status.TotalPickListWaitingTime.TotalMinutes / totalPickList;
        }

        /// <summary>
        /// Throughput total totes per hour
        /// </summary>
        /// <returns></returns>
        internal double GetToteThroughput()
        {
            double throughput = 0.0;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsItemTotes.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    throughput += sim.Status.TotalPickListsCompleted[type];
                }
            }

            var duration = sim.ClockTime - sim.Status.StartTime;

            throughput = throughput / duration.TotalHours;

            return throughput;
        }
        internal double GetCartThroughput()
        {
            double throughput = 0.0;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsOrderCarts.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    throughput += sim.Status.TotalPickListsCompleted[type];
                }
            }

            var duration = sim.ClockTime - sim.Status.StartTime;

            throughput = throughput / duration.TotalHours;

            return throughput;
        }

        internal int GetAllMinItemsPerTote()
        {
            int min = int.MaxValue;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsItemTotes.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    if (sim.Status.MinPickListSize[type] < min) min = sim.Status.MinPickListSize[type];
                }
            }
            if (min == int.MaxValue) min = 0;
            return min;
        }
        internal double GetAggregateAveItemsPerTote()
        {
            int totalPickList = 0;
            int totalPickJob = 0;

            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsItemTotes.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    totalPickList = sim.Status.TotalPickListsCompleted[type];
                    totalPickJob = sim.Status.TotalPickJobsCompleted[type];
                }
            }
            if (totalPickJob == 0) return 0.0;
            else return 1.0 * totalPickJob / totalPickList;
        }
        internal int GetAllMaxItemsPerTote()
        {
            int max = 0;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsItemTotes.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    if (sim.Status.MaxPickListSize[type] > max) max = sim.Status.MaxPickListSize[type];
                }
            }
            return max;
        }

        internal int GetAllMinItemsPerCart()
        {
            int min = int.MaxValue;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsOrderCarts.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    if (sim.Status.MinPickListSize[type] < min) min = sim.Status.MinPickListSize[type];
                }
            }
            if (min == int.MaxValue) min = 0;
            return min;
        }
        internal double GetAggregateAveItemsPerCart()
        {
            int totalPickList = 0;
            int totalPickJob = 0;

            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsOrderCarts.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    totalPickList = sim.Status.TotalPickListsCompleted[type];
                    totalPickJob = sim.Status.TotalPickJobsCompleted[type];
                }
            }

            if (totalPickJob == 0) return 0.0;
            else return 1.0 * totalPickJob / totalPickList;
        }
        internal int GetAllMaxItemsPerCart()
        {
            int max = 0;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsOrderCarts.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    if (sim.Status.MaxPickListSize[type] > max) max = sim.Status.MaxPickListSize[type];
                }
            }
            return max;
        }

        internal double GetAllMinToteUtilisation()
        {
            double min = double.PositiveInfinity;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsItemTotes.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    if (sim.Status.MinCartUtilisation[type] < min) min = sim.Status.MinCartUtilisation[type];
                }
            }
            if (double.IsPositiveInfinity(min)) return 0.0;
            return min;
        }
        internal double GetAggregateAveToteUtilisation()
        {
            List<double> allUtilisation = new List<double>();
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsItemTotes.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    allUtilisation.AddRange(sim.Status.AllCartUtilisation[type]);
                }
            }
            if (allUtilisation.Count == 0) return 0.0;
            else return allUtilisation.Average();
        }
        internal double GetAllMaxToteUtilisation()
        {
            double max = 0;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsItemTotes.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    if (sim.Status.MaxCartUtilisation[type] > max) max = sim.Status.MaxCartUtilisation[type];
                }
            }
            return max;
        }

        internal double GetAllMinCartUtilisation()
        {
            double min = double.PositiveInfinity;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsOrderCarts.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    if (sim.Status.MinCartUtilisation[type] < min) min = sim.Status.MinCartUtilisation[type];
                }
            }
            if (double.IsPositiveInfinity(min)) return 0.0;
            return min;
        }
        internal double GetAggregateAveCartUtilisation()
        {
            List<double> allUtilisation = new List<double>();
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsOrderCarts.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    allUtilisation.AddRange(sim.Status.AllCartUtilisation[type]);
                }
            }
            if (allUtilisation.Count == 0) return 0.0;
            else return allUtilisation.Average();
        }
        internal double GetAllMaxCartUtilisation()
        {
            double max = 0;
            foreach (var typeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                if (generator.PickerIdsOrderCarts.Contains(typeID))
                {
                    var type = sim.Scenario.GetPickerType[typeID];
                    if (sim.Status.MaxCartUtilisation[type] > max) max = sim.Status.MaxCartUtilisation[type];
                }
            }
            return max;
        }

        internal int GetNumOrdersToSorting()
        {
            int numOrders = 0;
            foreach (var PickerTypeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                // Sorting required
                if (PickerTypeID == PicklistGenerator.C_PickerID_MultiZone ||
                       PickerTypeID == PicklistGenerator.D_PickerID_MultiItem)
                {
                    var type = sim.Scenario.GetPickerType[PickerTypeID];
                    //numOrders += sim.Status.CompletedOrder[type].Count;
                    numOrders += generator.NumOrders[type];
                }
            }
            return numOrders;
        }

        internal int GetNumOrdersWithoutSorting()
        {
            int numOrders = 0;
            foreach (var PickerTypeID in generator.PickerIdsInStrategy[(PicklistGenerator.Strategy)strategy])
            {
                var type = sim.Scenario.GetPickerType[PickerTypeID];
                // No sorting required
                if (!(PickerTypeID == PicklistGenerator.C_PickerID_MultiZone ||
                       PickerTypeID == PicklistGenerator.D_PickerID_MultiItem))
                {
                    if (!(PickerTypeID == PicklistGenerator.B_PickerID_SingleItem ||
                            PickerTypeID == PicklistGenerator.C_PickerID_SingleItem ||
                            PickerTypeID == PicklistGenerator.D_PickerID_SingleItem))
                    {   // Non-single item
                        //numOrders += sim.Status.CompletedOrder[type].Count;
                        numOrders += generator.NumOrders[type];
                    }
                    else
                    {
                        // Single-item
                        numOrders += generator.NumOrders[type];
                    }
                }
            }
            return numOrders;
        }

        internal List<string> GetOutputHeaders()
        {
            List<string> headers = new List<string>();

            headers.Add("Scenario"); //[0]

            headers.Add("Average total cycle time per item (sec)"); //[1a]
            headers.Add("Average picking cycle time per item (sec)"); //[1b]
            headers.Add("Average batch completion time (min)"); //[2]
            headers.Add("Average tote throughput (totes/hour)"); //[3a]
            headers.Add("Average cart throughput (carts/hour)"); //[3b]
            headers.Add("Number of batches issued"); //[4]

            headers.Add("Min tote utilisation"); //[5a]
            headers.Add("Ave tote utilisation"); //[6a]
            headers.Add("Max tote utilisation"); //[7a]

            headers.Add("Min cart utilisation"); //[5b]
            headers.Add("Ave cart utilisation"); //[6b]
            headers.Add("Max cart utilisation"); //[7b]

            headers.Add("Min number of totes per batch"); //[8]
            headers.Add("Ave number of totes per batch"); //[9]
            headers.Add("Max number of totes per batch"); //[10]

            headers.Add("Min items per tote"); //[11a]
            headers.Add("Ave items per tote"); //[12a]
            headers.Add("Max items per tote"); //[13a]

            headers.Add("Min items per cart"); //[11b]
            headers.Add("Ave items per cart"); //[12b]
            headers.Add("Max items per cart"); //[13b]

            headers.Add("Ave number of active pickers"); //[14]
            headers.Add("Max number of active pickers"); //[15]

            headers.Add("Average waiting time for picking (min)"); //[16]
            headers.Add("Average waiting time for batch consolidation (min)"); //[17]

            headers.Add("Ave number of sorting stations open"); //[18]
            headers.Add("Max number of sorting stations open"); //[19]

            headers.Add("Ave number of batches in front of sorting stations"); //[20]
            headers.Add("Max number of batches in front of sorting stations"); //[21]

            headers.Add("Ave number of totes in front of sorting stations"); //[22]
            headers.Add("Max number of totes in front of sorting stations"); //[23]

            headers.Add("Number of orders to sorting"); //[24]
            headers.Add("Number of orders without sorting"); //[25]

            return headers;
        }
    }
}
