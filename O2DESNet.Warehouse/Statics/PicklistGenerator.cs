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

            MasterPickList.Clear();

            if (strategy == Strategy.A) StrategyA(scenario);
            if (strategy == Strategy.B) StrategyB(scenario);
            if (strategy == Strategy.C) StrategyC(scenario);
            if (strategy == Strategy.D) StrategyD(scenario);

            WriteToFiles(scenario);

            if (copyToScenario) CopyToScenario(scenario);
        }

        private static void WriteToFiles(Scenario scenario)
        {
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
        private static void CopyToScenario(Scenario scenario)
        {
            foreach (var type in scenario.MasterPickList.Keys)
            {
                scenario.MasterPickList[type].Clear();

                if (MasterPickList.ContainsKey(type))
                {
                    scenario.MasterPickList[type] = new List<List<PickJob>>(MasterPickList[type]);
                }
            }
        }

        /// <summary>
        /// Current strategy. Sequential assignment of orders. Only one PickerType.
        /// </summary>
        /// <param name="scenario"></param>
        private static void StrategyA(Scenario scenario)
        {
            // Assume only one picker type
            const string pickerType_ID = "Strategy_A_Picker";
            PickerType type = scenario.GetPickerType[pickerType_ID];
            MasterPickList.Add(type, new List<List<PickJob>>());

            List<Order> orders = AllOrders.Values.ToList();

            MasterPickList[type].Add(new List<PickJob>());

            while (orders.Count > 0)
            {
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
                        var rack = locations.First();
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

                    if (!reserved) throw new Exception("No available quantity for reservation for SKU " + item.SKU_ID);
                }
                // Next order
                orders.RemoveAt(0);
            }

            SortByLocation();
        }
        /// <summary>
        /// Hybrid Order Picking
        /// </summary>
        /// <param name="scenario"></param>
        private static void StrategyB(Scenario scenario)
        {
            throw new NotImplementedException("Strategy B not implemented!");
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

        #region Read Orders    
        /// <summary>
        /// CSV file in Orders folder
        /// </summary>
        /// <param name="filename"></param>
        public static void ReadOrders(string filename, Scenario scenario)
        {
            AllOrders.Clear();

            filename = @"Orders\" + filename;
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
