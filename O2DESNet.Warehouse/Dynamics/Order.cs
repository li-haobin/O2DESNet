using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    [Serializable]
    public class Order
    {
        public string Order_ID { get; set; }
        public List<SKU> Items { get; set; }
        public Dictionary<SKU, int> QtyRequired { get; set; }

        public Order(string id)
        {
            Order_ID = id;
            Items = new List<SKU>();
        }

        public bool IsSingleZoneFulfil(string zone)
        {
            return GetSingleZoneFulfil().Contains(zone);
        }

        public HashSet<string> GetSingleZoneFulfil()
        {
            if (Items.Count == 0) throw new Exception("Order is empty.");

            HashSet<string> output = new HashSet<string>(Items.First().GetFulfilmentZones());

            foreach (var item in Items)
            {
                output.IntersectWith(item.GetFulfilmentZones());
            }

            return output;
        }

        public HashSet<string> GetFulfilmentZones()
        {
            if (Items.Count == 0) throw new Exception("Order is empty.");

            HashSet<string> output = new HashSet<string>();

            foreach (var item in Items)
            {
                output.UnionWith(item.GetFulfilmentZones());
            }

            return output;
        }

        public void CountQtyRequired()
        {
            if (QtyRequired == null)
            {
                QtyRequired = new Dictionary<SKU, int>();

                foreach (var item in Items)
                {
                    if (!QtyRequired.ContainsKey(item)) QtyRequired.Add(item, 0);
                    QtyRequired[item]++;
                }
            }
        }
    }
}
