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
        /// Generate picklists to FILE, based on given strategy name
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="scenario"></param>
        public static void Generate(Strategy strategy, Scenario scenario)
        {
            if (AllOrders.Count == 0) throw new Exception("Orders have not been read! Read orders first.");

            if (strategy == Strategy.A) StrategyA(scenario);

            if (strategy == Strategy.B) StrategyB(scenario);

            if (strategy == Strategy.C) StrategyC(scenario);

            if (strategy == Strategy.D) StrategyD(scenario);

            WriteToFiles(scenario);
        }

        private static void WriteToFiles(Scenario scenario)
        {
            int count = 1;
            string filename = @"Picklist\" + scenario.Name + "_Picklist_" + count.ToString() + ".csv";

        }

        /// <summary>
        /// No zoning no grouping. Sequential assignment of orders. Only one PickerType.
        /// </summary>
        /// <param name="scenario"></param>
        private static void StrategyA(Scenario scenario)
        {
            const string pickerType_ID = "Strategy_A_Picker";
            PickerType type = scenario.GetPickerType[pickerType_ID];
            MasterPickList.Add(type, new List<List<PickJob>>());

            List<Order> orders = AllOrders.Values.ToList();

            MasterPickList[type].Add(new List<PickJob>());

            while (orders.Count > 0)
            {
                // If does not fit, create new picklist
                if (MasterPickList[type].Last().Count + orders.First().items.Count > type.Capacity)
                    MasterPickList[type].Add(new List<PickJob>());

                foreach (var item in orders.First().items)
                {
                    // Check item availability
                    // Reserve item at location
                    // Add to curPicklist:
                    // MasterPickList[type].Last().Add(new PickJob(item, location));

                }
                // Next order
                orders.RemoveAt(0);
            }
        }
        private static void StrategyB(Scenario scenario)
        {
            throw new NotImplementedException("Strategy B not implemented!");
        }
        private static void StrategyC(Scenario scenario)
        {
            throw new NotImplementedException("Strategy C not implemented!");
        }
        private static void StrategyD(Scenario scenario)
        {
            throw new NotImplementedException("Strategy D not implemented!");
        }
        #endregion


        #region Read Orders    
        /// <summary>
        /// CSV file in Orders folder
        /// </summary>
        /// <param name="filename"></param>
        public static void ReadOrders(string filename, Scenario scenario)
        {
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
            AllOrders[order_id].items.Add(sku);
        }
        #endregion
    }
}
