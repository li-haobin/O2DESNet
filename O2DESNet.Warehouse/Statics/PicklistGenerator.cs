using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Statics
{
    [Serializable]
    /// <summary>
    ///  class for generating picklists based on specified rule.
    /// Requires the use of Layout, SKU and inventory snapshot.
    /// </summary>
    public class PicklistGenerator
    {
        private Random rnd;

        public enum Strategy { A, B, C, D };

        public const string A_PickerID = "Strategy_A_Picker";

        public const string B_PickerID_SingleItem = "Strategy_B_SingleItem";
        public const string B_PickerID_SingleZone = "Strategy_B_SingleZone";
        public const string B_PickerID_MultiZone = "Strategy_B_MultiZone";

        public const string C_PickerID_SingleItem = "Strategy_C_SingleItem";
        public const string C_PickerID_SingleZone = "Strategy_C_SingleZone";
        public const string C_PickerID_MultiZone = "Strategy_C_MultiZone";

        public const string D_PickerID_SingleItem = "Strategy_D_SingleItem";
        public const string D_PickerID_MultiItem = "Strategy_D_MultiItem";

        public Dictionary<Strategy, List<string>> PickerIdsInStrategy { get; private set; }

        public HashSet<string> PickerIdsItemTotes { get; private set; }
        public HashSet<string> PickerIdsOrderCarts { get; private set; }

        public Dictionary<string, Order> AllOrders { get; private set; }
        public Dictionary<PickerType, int> NumOrders { get; private set; }
        public Dictionary<PickerType, List<PickList>> MasterPickList { get; private set; }

        // This is just purely by counting the valid orders. Not considering insufficient SKUs.
        public Dictionary<PickerType, int> NumOrdersALL { get; private set; }

        public int MasterBatchSize { get; private set; }

        public int orderCount { get; private set; }

        // For debug
        public HashSet<string> IncompleteOrder { get; private set; }
        public List<string> MissingSKU { get; private set; } // SKU in order but missing in inventory
        public List<string> InsufficientSKU { get; private set; } // Insufficient inventory

        public PicklistGenerator()
        {
            rnd = new Random(0);

            PickerIdsInStrategy = new Dictionary<Strategy, List<string>>();
            PickerIdsInStrategy.Add(Strategy.A, new List<string> { A_PickerID });
            PickerIdsInStrategy.Add(Strategy.B, new List<string> { B_PickerID_SingleZone, B_PickerID_MultiZone, B_PickerID_SingleItem });
            PickerIdsInStrategy.Add(Strategy.C, new List<string> { C_PickerID_SingleZone, C_PickerID_MultiZone, C_PickerID_SingleItem });
            PickerIdsInStrategy.Add(Strategy.D, new List<string> { D_PickerID_MultiItem, D_PickerID_SingleItem });

            PickerIdsItemTotes = new HashSet<string>();
            PickerIdsItemTotes.Add(B_PickerID_SingleItem);
            PickerIdsItemTotes.Add(C_PickerID_MultiZone);
            PickerIdsItemTotes.Add(C_PickerID_SingleItem);
            PickerIdsItemTotes.Add(D_PickerID_MultiItem);
            PickerIdsItemTotes.Add(D_PickerID_SingleItem);

            PickerIdsOrderCarts = new HashSet<string>();
            PickerIdsOrderCarts.Add(A_PickerID);
            PickerIdsOrderCarts.Add(B_PickerID_SingleZone);
            PickerIdsOrderCarts.Add(B_PickerID_MultiZone);
            PickerIdsOrderCarts.Add(C_PickerID_SingleZone);

            orderCount = 0;
            MasterBatchSize = IOHelper.MasterBatchSize;
        }

        #region Picklist generation
        /// <summary>
        /// Generate picklists to FILE, based on given strategy. Optional copy to scenario directly.
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="scenario"></param>
        public void Generate(Strategy strategy, Scenario scenario, bool copyToScenario = false, bool writeToFile = false)
        {
            if (AllOrders.Count == 0) throw new Exception("Orders have not been read! Read orders first.");

            MasterPickList = new Dictionary<PickerType, List<PickList>>();
            NumOrders = new Dictionary<PickerType, int>();
            NumOrdersALL = new Dictionary<PickerType, int>();

            // Debug
            if (IOHelper.OrderCount == null)
            {
                IOHelper.OrderCount = new Dictionary<Strategy, List<int>>();
                IOHelper.OrderCount.Add(Strategy.A, new List<int>());
                IOHelper.OrderCount.Add(Strategy.B, new List<int>());
                IOHelper.OrderCount.Add(Strategy.C, new List<int>());
                IOHelper.OrderCount.Add(Strategy.D, new List<int>());
            }
            // End debug

            if (strategy == Strategy.A) StrategyA(scenario);
            if (strategy == Strategy.B) StrategyB(scenario);
            if (strategy == Strategy.C) StrategyC(scenario);
            if (strategy == Strategy.D) StrategyD(scenario);

            SortByLocation();

            if (writeToFile) WriteToFiles(scenario);

            if (copyToScenario) CopyToScenario(scenario);

            // For debug
            using (StreamWriter sw = new StreamWriter(@"Outputs\" + scenario.Name + "_InsufficientSKUs.csv"))
            {
                foreach (var sku_id in InsufficientSKU)
                {
                    sw.WriteLine(sku_id);
                }
            }

            // ResolveInsufficientSKU(scenario.Name);
        }
        /// <summary>
        /// Write MasterPickList to files
        /// </summary>
        /// <param name="scenario"></param>
        private void WriteToFiles(Scenario scenario)
        {
            DeletePicklistFiles(scenario);

            int count = 1;
            string filename = @"Picklist\" + scenario.Name + "_Picklist_" + count.ToString() + ".csv";

            foreach (var pickerType in MasterPickList)
            {
                var type_ID = pickerType.Key.PickerType_ID;
                var typePicklists = pickerType.Value;

                foreach (var picklist in typePicklists)
                {
                    // One picklist one file
                    using (StreamWriter output = new StreamWriter(filename))
                    {
                        output.WriteLine(type_ID); // First line is PickerType_ID
                        foreach (var pickJob in picklist.pickJobs)
                        {
                            output.WriteLine("{0},{1},{2}", pickJob.item.SKU_ID, pickJob.rack.Rack_ID, pickJob.quantity);
                        }
                    }

                    count++;
                    filename = @"Picklist\" + scenario.Name + "_Picklist_" + count.ToString() + ".csv";
                }
            }
        }
        /// <summary>
        /// Copy MasterPickList to scenario.MasterPickList
        /// </summary>
        /// <param name="scenario"></param>
        private void CopyToScenario(Scenario scenario)
        {
            var types = scenario.MasterPickList.Keys.ToList();

            for (int i = 0; i < types.Count; i++)
            {
                scenario.MasterPickList[types[i]].Clear();

                if (MasterPickList.ContainsKey(types[i]))
                {
                    scenario.MasterPickList[types[i]] = MasterPickList[types[i]];
                }
            }
        }
        private void DeletePicklistFiles(Scenario scenario)
        {
            int count = 1;
            string filename = @"Picklist\" + scenario.Name + "_Picklist_" + count.ToString() + ".csv";

            while (File.Exists(filename))
            {
                File.Delete(filename);
                count++;
                filename = @"Picklist\" + scenario.Name + "_Picklist_" + count.ToString() + ".csv";
            }

        }
        /// <summary>
        /// Sort each picklist by location (PickJob.CPRack.Rack_ID)
        /// </summary>
        private void SortByLocation()
        {
            foreach (var type in MasterPickList.Keys)
            {
                var typePicklists = MasterPickList[type];

                for (int i = 0; i < typePicklists.Count; i++)
                {
                    typePicklists[i].pickJobs = typePicklists[i].pickJobs.OrderBy(job => job.rack.Rack_ID).ToList();
                }
            }
        }
        #endregion

        #region Strategies

        /// <summary>
        /// Current strategy. Sequential assignment of orders. Only one PickerType.
        /// </summary>
        /// <param name="scenario"></param>
        private void StrategyA(Scenario scenario)
        {
            List<Order> orders = AllOrders.Values.ToList();
            List<Order> unfulfilledOrders = new List<Order>();

            IOHelper.OrderCount[Strategy.A].Add(orders.Count);

            while (orders.Count > 0)
            {
                int OrderQty = Math.Min(MasterBatchSize, orders.Count);
                List<Order> releasedOrders = orders.ExtractRange(0, OrderQty);

                // Assume only one picker type A_Picker
                var unfulfilled = GeneratePicklistsFromOrders(scenario, releasedOrders, A_PickerID); // Order-based
                unfulfilledOrders.AddRange(unfulfilled);
            }
            //var test = unfulfilledOrders.ExtractAll(o => o.Items.Count == 0);
            //if (test.Count > 0) throw new Exception("What???");

            IOHelper.OrderCount[Strategy.A].Add(unfulfilledOrders.Count);
        }
        /// <summary>
        /// Hybrid Order Picking
        /// </summary>
        /// <param name="scenario"></param>
        private void StrategyB(Scenario scenario)
        {
            List<Order> orders = AllOrders.Values.ToList();
            List<Order> unfulfilledOrders = new List<Order>();
            int singleItemUnfulfilled = 0;

            IOHelper.OrderCount[Strategy.B].Add(orders.Count);

            while (orders.Count > 0)
            {
                int OrderQty = Math.Min(MasterBatchSize, orders.Count);
                List<Order> releasedOrders = orders.ExtractRange(0, OrderQty);

                List<Order> singleItemOrders = ExtractSingleItemOrders(releasedOrders);
                IOHelper.OrderCount[Strategy.B].Add(releasedOrders.Count);

                GenerateSingleZoneOrders(scenario, releasedOrders, B_PickerID_SingleZone); // Order-based
                IOHelper.OrderCount[Strategy.B].Add(releasedOrders.Count);

                // Remaining order in List orders are multi-zone orders
                var unfulfilled = GeneratePicklistsFromOrders(scenario, releasedOrders, B_PickerID_MultiZone); // Order-based
                unfulfilledOrders.AddRange(unfulfilled);

                // Single-Item orders last
                //GeneratePicklistsFromOrders(scenario, singleItemOrders, B_PickerID_SingleItem);
                singleItemUnfulfilled += GenerateSingleItemOrders(scenario, singleItemOrders, B_PickerID_SingleItem); // Item-based
            }

            IOHelper.OrderCount[Strategy.B].Add(unfulfilledOrders.Count);
            IOHelper.OrderCount[Strategy.B].Add(singleItemUnfulfilled);
        }
        /// <summary>
        /// Hybrid Zone Picking
        /// </summary>
        /// <param name="scenario"></param>
        private void StrategyC(Scenario scenario)
        {
            List<Order> orders = AllOrders.Values.ToList();
            int singleItemUnfulfilled = 0;

            IOHelper.OrderCount[Strategy.C].Add(orders.Count);

            while (orders.Count > 0)
            {
                int OrderQty = Math.Min(MasterBatchSize, orders.Count);
                List<Order> releasedOrders = orders.ExtractRange(0, OrderQty);

                List<Order> singleItemOrders = ExtractSingleItemOrders(releasedOrders);

                IOHelper.OrderCount[Strategy.C].Add(releasedOrders.Count);
                GenerateSingleZoneOrders(scenario, releasedOrders, C_PickerID_SingleZone); // Order-based

                // Split remaining orders into zones
                IOHelper.OrderCount[Strategy.C].Add(releasedOrders.Count);
                GeneratePureZoneOrders(scenario, releasedOrders, C_PickerID_MultiZone); // Item-based
                IOHelper.OrderCount[Strategy.C].Add(releasedOrders.Count);

                // Single-Item orders last
                //GeneratePicklistsFromOrders(scenario, singleItemOrders, C_PickerID_SingleItem);
                singleItemUnfulfilled += GenerateSingleItemOrders(scenario, singleItemOrders, C_PickerID_SingleItem); // Item-based
            }

            IOHelper.OrderCount[Strategy.C].Add(singleItemUnfulfilled);
        }
        /// <summary>
        /// Pure Zone Picking
        /// </summary>
        /// <param name="scenario"></param>
        private void StrategyD(Scenario scenario)
        {
            List<Order> orders = AllOrders.Values.ToList();
            int singleItemUnfulfilled = 0;

            IOHelper.OrderCount[Strategy.D].Add(orders.Count);

            while (orders.Count > 0)
            {
                int OrderQty = Math.Min(MasterBatchSize, orders.Count);
                List<Order> releasedOrders = orders.ExtractRange(0, OrderQty);

                List<Order> singleItemOrders = ExtractSingleItemOrders(releasedOrders);

                IOHelper.OrderCount[Strategy.D].Add(releasedOrders.Count);
                // Split remaining orders into zones (Multi-Item)
                GeneratePureZoneOrders(scenario, releasedOrders, D_PickerID_MultiItem); // Item-based
                IOHelper.OrderCount[Strategy.D].Add(releasedOrders.Count);

                // Single-Item orders last
                //GeneratePicklistsFromOrders(scenario, singleItemOrders, D_PickerID_SingleItem);
                singleItemUnfulfilled += GenerateSingleItemOrders(scenario, singleItemOrders, D_PickerID_SingleItem); // Item-based
            }

            IOHelper.OrderCount[Strategy.D].Add(singleItemUnfulfilled);

        }

        /// <summary>
        /// Item-based. Return unfulfilled count.
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="orders"></param>
        /// <param name="pickerID"></param>
        private int GenerateSingleItemOrders(Scenario scenario, List<Order> orders, string pickerID)
        {
            var type = scenario.GetPickerType[pickerID];
            List<SKU> singleItem = orders.SelectMany(o => o.Items).ToList();
            var unfulfilled = GeneratePicklistsFromItems(scenario, singleItem, pickerID);

            if (!NumOrders.ContainsKey(type)) NumOrders.Add(type, 0);
            NumOrders[type] = NumOrders[type] + orders.Count - unfulfilled.Count; // Ok, because 1 order 1 item

            if (!NumOrdersALL.ContainsKey(type)) NumOrdersALL.Add(type, 0);
            NumOrdersALL[type] += orders.Count;

            return unfulfilled.Count;
        }

        /// <summary>
        /// Item-based. Generate picklists for pure zone orders. No orders should remain.
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="orders"></param>
        private void GeneratePureZoneOrders(Scenario scenario, List<Order> orders, string pickerID)
        {
            int maxOrderBatchSize = IOHelper.MaxOrderBatchSize;

            var type = scenario.GetPickerType[pickerID];
            if (!MasterPickList.ContainsKey(type)) MasterPickList.Add(type, new List<PickList>());

            if (!NumOrders.ContainsKey(type)) NumOrders.Add(type, 0);
            NumOrders[type] += orders.Count; // TODO: Maybe at the end of all these, there will be some orders unprocessed?

            if (!NumOrdersALL.ContainsKey(type)) NumOrdersALL.Add(type, 0);
            NumOrdersALL[type] += orders.Count;

            // OrderBatching
            while (orders.Count > 0)
            {
                orders = orders.OrderBy(o => o.GetFulfilmentZones().Count).ToList(); // Increasing fragmentation

                // Start of batch
                // Get first min(maxOrderPerBatch, orders.Count)
                int batchQty = Math.Min(maxOrderBatchSize, orders.Count);
                List<Order> ordersBatch = orders.ExtractRange(0, batchQty);

                HashSet<string> allZones = GetFulfilmentZones(ordersBatch);
                // This is where order information is lost
                List<SKU> items = ordersBatch.SelectMany(order => order.Items).ToList(); // Flattening items from orders, mutable

                int startPickListCount = MasterPickList[type].Count;
                if (allZones.Count > 0) // By right this should always happen, unless a whole batch contains insufficient SKU
                {
                    // Start new OrderBatch
                    scenario.OrderBatches.Add(new OrderBatch(ordersBatch));
                    MasterPickList[type].Add(new PickList());
                    scenario.OrderBatches.Last().PickLists.Add(MasterPickList[type].Last());

                    foreach (var zone in allZones) // for each zone, one set of picklists
                    {
                        var oneZone = items.ExtractAll(item => item.IsFulfiledZone(zone)); // Potentially fulfilled in this zone

                        var unfulfilled = GeneratePicklistsFromItems(scenario, oneZone, pickerID, zone);

                        items.AddRange(unfulfilled); // Append back unfulfilled items
                    }

                    foreach (var picklist in scenario.OrderBatches.Last().PickLists)
                    {
                        scenario.WhichOrderBatch.Add(picklist, scenario.OrderBatches.Last());
                    }
                }
                else // a whole batch contains insufficient SKU
                {
                    NumOrders[type] -= ordersBatch.Count;
                }

                // End of batch
                int numPickListGenerated = MasterPickList[type].Count - startPickListCount;

                // Any order remaining means insufficient
                if (items.Count > 0) //throw new Exception("There are still item left!");
                    InsufficientSKU.AddRange(items.Select(i => i.SKU_ID));
            }
        }
        /// <summary>
        /// Order-based. Generate picklists for single zone orders. Remaining orders in List are unfulfilled.
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="orders"></param>
        private void GenerateSingleZoneOrders(Scenario scenario, List<Order> orders, string pickerID)
        {
            // Determine orders with items in a single zone

            var type = scenario.GetPickerType[pickerID];

            HashSet<string> allZones = GetFulfilmentZones(orders);
            // Find single-zone orders
            foreach (var zone in allZones)
            {
                List<Order> zoneOrders = orders.ExtractAll(order => order.IsSingleZoneFulfil(zone)); // Potentially fulfiled in zone

                var unfulfilled = GeneratePicklistsFromOrders(scenario, zoneOrders, pickerID, zone); // Reservation done here

                orders.AddRange(unfulfilled); // Append back unfulfilled orders
            }
        }

        /// <summary>
        /// Determine the zones where the orders can be fulfilled
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        private HashSet<string> GetFulfilmentZones(List<Order> orders)
        {
            // Init zones of interest
            HashSet<string> allZones = new HashSet<string>();
            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    allZones.UnionWith(item.GetFulfilmentZones());
                }
            }

            return allZones;
        }
        /// <summary>
        /// Generate picklists for specified picker type from given set of orders. Optional only from single zone. Return unfulfilled items.
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="items"></param>
        /// <param name="pickerType_ID"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        private List<SKU> GeneratePicklistsFromItems(Scenario scenario, List<SKU> items, string pickerType_ID, string zone = null)
        {
            // Pure zone calls this method with zone != null

            List<SKU> unfulfilledItems = new List<SKU>();

            var type = scenario.GetPickerType[pickerType_ID];
            if (!MasterPickList.ContainsKey(type)) MasterPickList.Add(type, new List<PickList>());

            if (MasterPickList[type].Count == 0 || MasterPickList[type].Last().pickJobs.Count > 0)
            {
                MasterPickList[type].Add(new PickList());
                if (zone != null) scenario.OrderBatches.Last().PickLists.Add(MasterPickList[type].Last());
            }

            while (items.Count > 0)
            {
                if (zone != null && !items.First().IsFulfiledZone(zone))
                {
                    unfulfilledItems.Add(items.First());
                }
                else
                {
                    // If does not fit, create new picklist
                    if (MasterPickList[type].Count == 0 || MasterPickList[type].Last().pickJobs.Count >= type.Capacity)
                    {
                        MasterPickList[type].Add(new PickList());
                        if (zone != null) scenario.OrderBatches.Last().PickLists.Add(MasterPickList[type].Last());
                    }

                    // Process item
                    bool isReserved = ReserveItem(type, items.First(), zone);
                    if (!isReserved)
                    {
                        unfulfilledItems.Add(items.First()); // document as unfulfilled

                        InsufficientSKU.Add(items.First().SKU_ID); // Add to insufficient
                                                                   // throw new Exception("No available quantity for reservation for SKU " + items.First().SKU_ID);
                    }
                }
                // Next item
                items.RemoveAt(0);
            }

            if (MasterPickList[type].Last().ItemsCount == 0) // Safety
            {
                if (zone != null) scenario.OrderBatches.Last().PickLists.Remove(MasterPickList[type].Last());
                MasterPickList[type].RemoveAt(MasterPickList[type].Count - 1);
            }

            return unfulfilledItems;
        }
        /// <summary>
        /// Generate picklists for specified picker type from given set of orders. Optional only from single zone. Return unfulfilled orders.
        /// </summary>
        /// <param name="pickerType_ID"></param>
        /// <param name="orders"></param>
        /// <param name="scenario"></param>
        private List<Order> GeneratePicklistsFromOrders(Scenario scenario, List<Order> orders, string pickerType_ID, string zone = null)
        {
            List<Order> unfulfilledOrders = new List<Order>();

            var type = scenario.GetPickerType[pickerType_ID];
            if (!MasterPickList.ContainsKey(type)) MasterPickList.Add(type, new List<PickList>());
            if (!NumOrders.ContainsKey(type)) NumOrders.Add(type, 0);
            //NumOrders[type] += orders.Count;

            if (!NumOrdersALL.ContainsKey(type)) NumOrdersALL.Add(type, 0);
            NumOrdersALL[type] += orders.Count;

            if (MasterPickList[type].Count == 0 || MasterPickList[type].Last().pickJobs.Count > 0)
            {
                MasterPickList[type].Add(new PickList());
                orderCount = 0;
            }

            while (orders.Count > 0)
            {
                if (zone != null && !orders.First().IsSingleZoneFulfil(zone))
                {
                    unfulfilledOrders.Add(orders.First());
                }
                else
                {
                    // If does not fit, create new picklist, Capacity is number of orders.
                    if (MasterPickList[type].Count == 0
                         || orderCount >= type.Capacity)
                    // || MasterPickList[type].Last().Count + orders.First().Items.Count > type.Capacity)
                    {
                        MasterPickList[type].Add(new PickList());
                        orderCount = 0;
                    }

                    // These two seriously just because there are insufficient items
                    var prevPickJobCount = MasterPickList[type].Last().pickJobs.Count;
                    bool atLeastOneItem = false;

                    // Process items in current order
                    foreach (var item in orders.First().Items)
                    {
                        bool isReserved = ReserveItem(type, item, zone);

                        if (!isReserved)
                        {
                            InsufficientSKU.Add(item.SKU_ID); // Add to insufficient
                                                              // throw new Exception("No available quantity for reservation for SKU " + item.SKU_ID);
                        }
                        else
                        {
                            atLeastOneItem = true;
                        }
                    }

                    // Skip if none of the items in the order is found
                    //if (MasterPickList[type].Last().pickJobs.Count > prevPickJobCount)
                    if (atLeastOneItem)
                    {
                        // Should only count if the order is processed
                        orderCount++;
                        NumOrders[type]++;
                        MasterPickList[type].Last().orders.Add(orders.First());
                    }
                    else
                    {
                        unfulfilledOrders.Add(orders.First());
                    }
                }

                // Next order
                orders.RemoveAt(0);
            }

            //NumOrders[type] -= unfulfilledOrders.Count;

            // Trim master picklist, safety
            if (MasterPickList[type].Last().ItemsCount == 0)
            {
                MasterPickList[type].RemoveAt(MasterPickList[type].Count - 1);
            }

            NumOrdersALL[type] -= unfulfilledOrders.Count;

            return unfulfilledOrders;
        }

        /// <summary>
        /// Inventory reservation procedure
        /// </summary>
        /// <param name="type"></param>
        /// <param name="item"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        private bool ReserveItem(PickerType type, SKU item, string zone = null)
        {
            var locations = item.QtyAtRack.Keys.ToList();
            bool reserved = false;
            // Inventory reservation procedure
            while (locations.Count > 0 && !reserved)
            {
                // Check item availability
                var rack = locations.First(); // ONLY FROM DESIRED ZONE
                if (zone != null && zone != rack.GetZone()) locations.RemoveAt(0);
                else
                {
                    if (item.GetQtyAvailable(rack) > 0)
                    {
                        // Reserve item at location
                        item.ReserveFromRack(rack);
                        // Add to curPicklist
                        MasterPickList[type].Last().Add(new PickJob(item, rack));
                        reserved = true;
                    }
                    else
                    {
                        locations.RemoveAt(0);
                    }
                }
            }

            return reserved;
        }
        private List<Order> ExtractSingleItemOrders(List<Order> orders)
        {
            return orders.ExtractAll(order => order.Items.Count == 1);
        }
        /// <summary>
        /// List extension to extract elements defined by predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="match"></param>
        /// <returns></returns>

        #endregion

        #region Read Orders    
        /// <summary>
        /// CSV file in Picklist folder
        /// </summary>
        /// <param name="filename"></param>
        public void ReadOrders(Scenario scenario, string filename, bool autoResolve = false)
        {
            // scenario.SKUs contain all SKUs already read

            // For debug
            IncompleteOrder = new HashSet<string>();
            MissingSKU = new List<string>();
            InsufficientSKU = new List<string>();

            AllOrders = new Dictionary<string, Order>();

            filename = @"Inputs\" + filename;
            if (!File.Exists(filename))
                throw new Exception("Order file " + filename + " does not exist");

            using (StreamReader sr = new StreamReader(filename))
            {
                string line = sr.ReadLine(); // First line header: Order_ID, SKU

                while ((line = sr.ReadLine()) != null)
                {
                    var data = line.Split(',');
                    var order_id = data[0];
                    var sku = data[1];

                    AddOrCreateOrder(scenario, order_id, sku, autoResolve);
                }
            }

            // If auto resolved, all orders should be complete...
            if (autoResolve)
            {
                if (IncompleteOrder.Count > 0) throw new Exception("All orders should be complete since auto resolved");

                ForceAvailability(scenario, autoResolve);
            }

            // Remove incomplete order
            foreach (var order in IncompleteOrder)
            {
                AllOrders.Remove(order);
            }

            // For debug, write Missing SKU into file
            using (StreamWriter sw = new StreamWriter(@"Outputs\" + scenario.Name + "_MissingSKUs.csv"))
            {
                foreach (var sku_id in MissingSKU)
                {
                    sw.WriteLine(sku_id);
                }
            }


            scenario.Orders = new Dictionary<string, Order>(AllOrders);
        }

        private void AddOrCreateOrder(Scenario scenario, string order_id, string sku_id, bool autoResolve = false)
        {
            // Auto Resolve: add missing SKU required in order, assigned to a random location
            if (autoResolve && !scenario.SKUs.ContainsKey(sku_id))
            {
                // Record missing SKU
                MissingSKU.Add(sku_id);

                // Make SKU
                var newSKU = new SKU(sku_id);
                // Get random rack
                var rack = scenario.Racks.ElementAt(rnd.Next(scenario.Racks.Count)).Value;
                // Assign SKU to rack
                scenario.AddToRack(newSKU, rack);
            }

            if (!scenario.SKUs.ContainsKey(sku_id))
            {
                // Record missing SKU
                MissingSKU.Add(sku_id);
                IncompleteOrder.Add(order_id);
            }
            else
            {
                // Find SKU
                var sku = scenario.SKUs[sku_id];

                // Create new order
                if (!AllOrders.ContainsKey(order_id))
                {
                    AllOrders.Add(order_id, new Order(order_id));
                }

                var order = AllOrders[order_id];

                // Add SKU to order
                order.Items.Add(sku);

                // Add order to SKU
                if (!sku.QtyForOrder.ContainsKey(order)) sku.QtyForOrder.Add(order, 0);
                sku.QtyForOrder[order]++;
            }
        }
        #endregion

        // This is the old "brute-force" method
        public void ResolveInsufficientSKU(string scenarioName)
        {
            string insufficientFile = @"Outputs\" + scenarioName + "_InsufficientSKUs.csv";
            string SKUFile = @"Inputs\" + scenarioName + "_SKUs.csv";

            Dictionary<string, int> InsufficientSKU = new Dictionary<string, int>();
            Dictionary<string, string> Layout = new Dictionary<string, string>();

            using (StreamReader sr = new StreamReader(insufficientFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!InsufficientSKU.ContainsKey(line)) InsufficientSKU.Add(line, 0);

                    InsufficientSKU[line]++;
                }
            }

            if (InsufficientSKU.Count > 0)
            {

                using (StreamReader sr = new StreamReader(SKUFile))
                {
                    string line = sr.ReadLine();
                    while ((line = sr.ReadLine()) != null)
                    {
                        var data = line.Split(',');
                        string sku_id = data[0];
                        string loc = data[2];

                        if (!Layout.ContainsKey(sku_id)) Layout.Add(sku_id, loc);
                    }
                }

                List<string> AdditionalSKU = new List<string>();

                foreach (var sku in InsufficientSKU)
                {
                    for (int i = 0; i < sku.Value; i++)
                    {
                        AdditionalSKU.Add(sku.Key + ",," + Layout[sku.Key]);
                    }
                }

                File.AppendAllLines(SKUFile, AdditionalSKU);
            }
        }

        // orders: AllOrders
        // availability: scenario.SKUs
        private void ForceAvailability(Scenario scenario, bool autoResolve)
        {
            if (!autoResolve) throw new Exception("auto resolve not enabled");

            // Internal counters
            Dictionary<SKU, int> QtyRequired = new Dictionary<SKU, int>();
            Dictionary<SKU, int> QtyAvailable = new Dictionary<SKU, int>();

            // Calculate QtyRequired
            foreach (var order in AllOrders.Values)
            {
                order.CountQtyRequired();

                foreach (var kvp in order.QtyRequired)
                {
                    if (!QtyRequired.ContainsKey(kvp.Key)) QtyRequired.Add(kvp.Key, 0);

                    QtyRequired[kvp.Key] += kvp.Value;
                }
            }

            // Calculate QtyAvailable
            foreach (var sku in scenario.SKUs.Values)
            {
                QtyAvailable.Add(sku, sku.GetTotalQty());
            }

            // Make everything available
            foreach (var sku in QtyRequired.Keys)
            {
                var additionalQty = QtyRequired[sku] - QtyAvailable[sku];
                if (additionalQty > 0)
                {
                    var atRacks = sku.QtyAtRack.Keys;
                    var rack = atRacks.ElementAt(rnd.Next(atRacks.Count));
                    sku.AddToRack(rack, additionalQty);
                }
            }
        }
    }
}
