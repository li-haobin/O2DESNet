using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Statics
{
    /// <summary>
    /// Static class for generating picklists based on specified rule.
    /// Requires the use of Layout, SKU and inventory snapshot.
    /// </summary>
    public static class PicklistGenerator
    {
        public enum Strategy { A, B, C, D };
        const string A_PickerID = "Strategy_A_Picker";
        const string C_PickerID_SingleItem = "Strategy_C_SingleItem";
        const string C_PickerID_SingleZone = "Strategy_C_SingleZone";
        const string C_PickerID_MultiZone = "Strategy_C_MultiZone";

        public static Dictionary<string, Order> AllOrders { get; private set; }
        public static Dictionary<PickerType, List<List<PickJob>>> MasterPickList { get; private set; }

        #region Picklist generation
        /// <summary>
        /// Generate picklists to FILE, based on given strategy. Optional copy to scenario directly.
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="scenario"></param>
        public static void Generate(Strategy strategy, Scenario scenario, bool copyToScenario = false)
        {
            if (AllOrders.Count == 0) throw new Exception("Orders have not been read! Read orders first.");

            MasterPickList = new Dictionary<PickerType, List<List<PickJob>>>();

            if (strategy == Strategy.A) StrategyA(scenario);
            if (strategy == Strategy.B) StrategyB(scenario);
            if (strategy == Strategy.C) StrategyC(scenario);
            if (strategy == Strategy.D) StrategyD(scenario);

            SortByLocation();

            WriteToFiles(scenario);

            if (copyToScenario) CopyToScenario(scenario);
        }
        /// <summary>
        /// Write MasterPickList to files
        /// </summary>
        /// <param name="scenario"></param>
        private static void WriteToFiles(Scenario scenario)
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
                        foreach (var pickJob in picklist)
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
        private static void CopyToScenario(Scenario scenario)
        {
            var types = scenario.MasterPickList.Keys.ToList();

            for (int i = 0; i < types.Count; i++)
            {
                scenario.MasterPickList[types[i]].Clear();

                if (MasterPickList.ContainsKey(types[i]))
                {
                    scenario.MasterPickList[types[i]] = new List<List<PickJob>>(MasterPickList[types[i]]);
                }
            }
        }
        private static void DeletePicklistFiles(Scenario scenario)
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
        private static void SortByLocation()
        {
            foreach (var type in MasterPickList.Keys)
            {
                var typePicklists = MasterPickList[type];

                for (int i = 0; i < typePicklists.Count; i++)
                {
                    typePicklists[i] = typePicklists[i].OrderBy(o => o.rack.Rack_ID).ToList();
                }
            }
        }
        #endregion

        #region Strategies

        /// <summary>
        /// Current strategy. Sequential assignment of orders. Only one PickerType.
        /// </summary>
        /// <param name="scenario"></param>
        private static void StrategyA(Scenario scenario)
        {
            List<Order> orders = AllOrders.Values.ToList();

            // Assume only one picker type A_Picker
            GeneratePicklists(A_PickerID, orders, scenario);
        }
        /// <summary>
        /// Hybrid Order Picking
        /// </summary>
        /// <param name="scenario"></param>
        private static void StrategyB(Scenario scenario)
        {
            // Init orders
            List<Order> orders = AllOrders.Values.ToList();

            // Separate single item orders
            List<Order> singleItemOrders = orders.ExtractAll(order => order.Items.Count == 1);

            // Determine orders with items in a single zone
            // Init zones of interest
            HashSet<string> allZones = new HashSet<string>();
            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    allZones.UnionWith(item.GetFulfilmentZones());
                }
            }
            // Find single-zone orders
            Dictionary<string, List<Order>> singleZoneOrders = new Dictionary<string, List<Order>>();
            foreach (var zone in allZones)
            {
                List<Order> zoneOrders = orders.ExtractAll(order => order.IsSingleZoneFulfil(zone)); // Potentially fulfiled in zone
                
                var unfilfilled = GeneratePicklists(C_PickerID_SingleZone, zoneOrders, scenario, zone); // Reservation done here

                orders.AddRange(unfilfilled); // Append back unfulfilled orders
                zoneOrders = zoneOrders.Except(unfilfilled).ToList(); // Remove unfulfilled orders

                singleZoneOrders.Add(zone, zoneOrders);
            }

            // Remaining order in List orders are multi-zone orders
            GeneratePicklists(C_PickerID_MultiZone, orders, scenario);
            // Single-Item orders last
            GeneratePicklists(C_PickerID_SingleItem, singleItemOrders, scenario);

        }
        /// <summary>
        /// Hybrid Zone Picking
        /// </summary>
        /// <param name="scenario"></param>
        private static void StrategyC(Scenario scenario)
        {
            throw new NotImplementedException("Strategy C not implemented!");
        }
        /// <summary>
        /// Pure Zone Picking
        /// </summary>
        /// <param name="scenario"></param>
        private static void StrategyD(Scenario scenario)
        {
            throw new NotImplementedException("Strategy D not implemented!");
        }

        // Need to have optional generate only from certain zone for Strategy B
        /// <summary>
        /// Generate picklists for specified picker type from given set of orders
        /// </summary>
        /// <param name="pickerType_ID"></param>
        /// <param name="orders"></param>
        /// <param name="scenario"></param>
        private static List<Order> GeneratePicklists(string pickerType_ID, List<Order> orders, Scenario scenario, string zone = null)
        {
            List<Order> unfulfilledOrders = new List<Order>();

            var type = scenario.GetPickerType[pickerType_ID];

            if (!MasterPickList.ContainsKey(type))
                MasterPickList.Add(type, new List<List<PickJob>>());

            while (orders.Count > 0)
            {
                if (zone != null && !orders.First().IsSingleZoneFulfil(zone))
                {
                    unfulfilledOrders.Add(orders.First());
                }
                else
                {
                    MasterPickList[type].Add(new List<PickJob>());

                    // If does not fit, create new picklist
                    if (MasterPickList[type].Last().Count + orders.First().Items.Count > type.Capacity)
                        MasterPickList[type].Add(new List<PickJob>());

                    // Process items in current order
                    foreach (var item in orders.First().Items)
                    {
                        var locations = item.QtyAtRack.Keys.ToList();
                        bool reserved = false;
                        // Inventory reservation procedure
                        while (locations.Count > 0 && !reserved)
                        {
                            // Check item availability
                            var rack = locations.First(); // HERE! ONLY FROM DESIRED ZONE
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

                        if (!reserved) throw new Exception("No available quantity for reservation for SKU " + item.SKU_ID);
                    }

                }
                // Next order
                orders.RemoveAt(0);
            }

            return unfulfilledOrders;
        }
        /// <summary>
        /// List extension to extract elements defined by predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        private static List<T> ExtractAll<T>(this List<T> source, Predicate<T> match)
        {
            List<T> extract = source.FindAll(match);
            source.RemoveAll(match);
            return extract;
        }
        #endregion

        #region Read Orders    
        /// <summary>
        /// CSV file in Picklist folder
        /// </summary>
        /// <param name="filename"></param>
        public static void ReadOrders(string filename, Scenario scenario)
        {
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

                    AddOrCreateOrder(id, sku, scenario);
                }
            }
        }
        private static void AddOrCreateOrder(string order_id, string sku_id, Scenario scenario)
        {
            // Find SKU
            var sku = scenario.SKUs[sku_id];

            // New order
            if (!AllOrders.ContainsKey(order_id))
            {
                AllOrders.Add(order_id, new Order(order_id));
            }

            // Add SKU to order
            AllOrders[order_id].Items.Add(sku);
        }
        #endregion
    }
}
