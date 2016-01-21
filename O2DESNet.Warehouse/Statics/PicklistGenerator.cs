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

        public Dictionary<string, Order> AllOrders { get; private set; }
        public Dictionary<PickerType, int> NumOrders { get; private set; }
        public Dictionary<PickerType, List<PickList>> MasterPickList { get; private set; }

        private int orderCount = 0;

        // For debug
        public HashSet<string> IncompleteOrder { get; private set; }
        public List<string> MissingSKU { get; private set; } // SKU in order but missing in inventory
        public List<string> InsufficientSKU { get; private set; } // Insufficient inventory

        public PicklistGenerator()
        {

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

            if (strategy == Strategy.A) StrategyA(scenario);
            if (strategy == Strategy.B) StrategyB(scenario);
            if (strategy == Strategy.C) StrategyC(scenario);
            if (strategy == Strategy.D) StrategyD(scenario);

            SortByLocation();

            if (writeToFile) WriteToFiles(scenario);

            if (copyToScenario) CopyToScenario(scenario);

            // For debug
            using (StreamWriter sw = new StreamWriter(@"Picklist\" + scenario.Name + "_InsufficientSKUs.csv"))
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

            // Assume only one picker type A_Picker
            GeneratePicklistsFromOrders(scenario, orders, A_PickerID); // Order-based
        }
        /// <summary>
        /// Hybrid Order Picking
        /// </summary>
        /// <param name="scenario"></param>
        private void StrategyB(Scenario scenario)
        {
            List<Order> orders = AllOrders.Values.ToList();
            List<Order> singleItemOrders = ExtractSingleItemOrders(orders);

            GenerateSingleZoneOrders(scenario, orders, B_PickerID_SingleZone); // Order-based

            // Remaining order in List orders are multi-zone orders
            GeneratePicklistsFromOrders(scenario, orders, B_PickerID_MultiZone); // Order-based

            // Single-Item orders last
            //GeneratePicklistsFromOrders(scenario, singleItemOrders, B_PickerID_SingleItem);
            GenerateSingleItemOrders(scenario, singleItemOrders, B_PickerID_SingleItem); // Item-based
        }
        /// <summary>
        /// Hybrid Zone Picking
        /// </summary>
        /// <param name="scenario"></param>
        private void StrategyC(Scenario scenario)
        {
            List<Order> orders = AllOrders.Values.ToList();

            List<Order> singleItemOrders = ExtractSingleItemOrders(orders);

            GenerateSingleZoneOrders(scenario, orders, C_PickerID_SingleZone); // Order-based

            // Split remaining orders into zones
            GeneratePureZoneOrders(scenario, orders, C_PickerID_MultiZone); // Item-based

            // Single-Item orders last
            //GeneratePicklistsFromOrders(scenario, singleItemOrders, C_PickerID_SingleItem);
            GenerateSingleItemOrders(scenario, singleItemOrders, C_PickerID_SingleItem); // Item-based
        }
        /// <summary>
        /// Pure Zone Picking
        /// </summary>
        /// <param name="scenario"></param>
        private void StrategyD(Scenario scenario)
        {
            List<Order> orders = AllOrders.Values.ToList();

            List<Order> singleItemOrders = ExtractSingleItemOrders(orders);

            // Split remaining orders into zones
            GeneratePureZoneOrders(scenario, orders, D_PickerID_MultiItem); // Item-based

            // Single-Item orders last
            //GeneratePicklistsFromOrders(scenario, singleItemOrders, D_PickerID_SingleItem);
            GenerateSingleItemOrders(scenario, singleItemOrders, D_PickerID_SingleItem); // Item-based

        }

        /// <summary>
        /// Item-based.
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="orders"></param>
        /// <param name="pickerID"></param>
        private void GenerateSingleItemOrders(Scenario scenario, List<Order> orders, string pickerID)
        {
            var type = scenario.GetPickerType[pickerID];
            List<SKU> singleItem = orders.SelectMany(o => o.Items).ToList();
            var unfulfilled = GeneratePicklistsFromItems(scenario, singleItem, pickerID);

            if (!NumOrders.ContainsKey(type)) NumOrders.Add(type, 0);
            NumOrders[type] = NumOrders[type] + orders.Count - unfulfilled.Count;
        }

        /// <summary>
        /// Item-based. Generate picklists for pure zone orders. No orders should remain.
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="orders"></param>
        private void GeneratePureZoneOrders(Scenario scenario, List<Order> orders, string pickerID)
        {
            int maxOrdersBatchSize = 50; // This should be an input parameter

            var type = scenario.GetPickerType[pickerID];
            if (!MasterPickList.ContainsKey(type)) MasterPickList.Add(type, new List<PickList>());
            if (!NumOrders.ContainsKey(type)) NumOrders.Add(type, 0);
            NumOrders[type] += orders.Count;

            while (orders.Count > 0)
            {
                orders = orders.OrderBy(o => o.GetFulfilmentZones().Count).ToList(); // Increasing fragmentation

                // Start of batch
                // Get first min(maxOrderPerBatch, orders.Count)
                int batchQty = Math.Min(maxOrdersBatchSize, orders.Count);
                List<Order> ordersBatch = orders.ExtractRange(0, batchQty);

                HashSet<string> allZones = GetFulfilmentZones(ordersBatch);
                // This is where order information is lost
                List<SKU> items = ordersBatch.SelectMany(order => order.Items).ToList(); // Flattening items from orders

                int startPickListCount = MasterPickList[type].Count;
                if (allZones.Count > 0) // By right this should always happen, unless a whole batch contains insufficient SKU
                {
                    scenario.OrderBatches.Add(new OrderBatch(ordersBatch));
                    MasterPickList[type].Add(new PickList());
                    scenario.OrderBatches.Last().PickLists.Add(MasterPickList[type].Last());

                    foreach (var zone in allZones)
                    {
                        var oneZone = items.ExtractAll(item => item.IsFulfiledZone(zone)); // Potentially fulfilled in this zone

                        var unfulfilled = GeneratePicklistsFromItems(scenario, oneZone, pickerID, zone);

                        items.AddRange(unfulfilled); // Append back unfulfilled items
                    }

                    foreach(var picklist in scenario.OrderBatches.Last().PickLists)
                    {
                        scenario.WhichOrderBatch.Add(picklist, scenario.OrderBatches.Last());
                    }
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

            //int orderCount = 0; // BUG IS HERE

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

                    var prevPickJobCount = MasterPickList[type].Last().pickJobs.Count;

                    // Process items in current order
                    foreach (var item in orders.First().Items)
                    {
                        bool isReserved = ReserveItem(type, item, zone);

                        if (!isReserved)
                        {
                            InsufficientSKU.Add(item.SKU_ID); // Add to insufficient
                                                              // throw new Exception("No available quantity for reservation for SKU " + item.SKU_ID);
                        }
                    }

                    if (MasterPickList[type].Last().pickJobs.Count > prevPickJobCount)
                    {
                        // Should only count if the order is processed
                        orderCount++;
                        NumOrders[type]++;
                    }
                }
                // Next order
                orders.RemoveAt(0);
            }

            //NumOrders[type] -= unfulfilledOrders.Count;

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
        public void ReadOrders(Scenario scenario, string filename)
        {
            // For debug
            IncompleteOrder = new HashSet<string>();
            MissingSKU = new List<string>();
            InsufficientSKU = new List<string>();

            AllOrders = new Dictionary<string, Order>();

            filename = @"Picklist\" + filename;
            if (!File.Exists(filename))
                throw new Exception("Order file " + filename + " does not exist");

            using (StreamReader sr = new StreamReader(filename))
            {
                string line = sr.ReadLine(); // First line header: Order_ID, SKU

                while ((line = sr.ReadLine()) != null)
                {
                    var data = line.Split(',');
                    var id = data[0];
                    var sku = data[1];

                    AddOrCreateOrder(scenario, id, sku);
                }
            }

            // Remove incomplete order
            foreach (var order in IncompleteOrder)
            {
                AllOrders.Remove(order);
            }

            // For debug, write Missing SKU into file
            using (StreamWriter sw = new StreamWriter(@"Picklist\" + scenario.Name + "_MissingSKUs.csv"))
            {
                foreach (var sku_id in MissingSKU)
                {
                    sw.WriteLine(sku_id);
                }
            }


            scenario.Orders = new Dictionary<string, Order>(AllOrders);
        }
        private void AddOrCreateOrder(Scenario scenario, string order_id, string sku_id)
        {
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

                // New order
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

        public void ResolveInsufficientSKU(string scenarioName)
        {
            string insufficientFile = @"Picklist\" + scenarioName + "_InsufficientSKUs.csv";
            string SKUFile = @"Layout\" + scenarioName + "_SKUs.csv";

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
    }
}
