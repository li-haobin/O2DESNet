using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    [Serializable]
    public class SKU
    {
        public string SKU_ID { get; private set; }
        public string Description { get; private set; }
        public Dictionary<CPRack, int> QtyAtRack { get; private set; } // This makes it dynamic actually...
        public Dictionary<CPRack, int> ReservedAtRack { get; private set; } // For PicklistGenerator virtual reservation
        public Dictionary<Order, int> QtyForOrder { get; private set; } // For consolidation

        public SKU(string sku_id, string description = "")
        {
            SKU_ID = sku_id;
            Description = description;
            QtyAtRack = new Dictionary<CPRack, int>();
            ReservedAtRack = new Dictionary<CPRack, int>();
            QtyForOrder = new Dictionary<Order, int>();
        }

        public void AddToRack(CPRack rack, int quantity = 1)
        {
            if (QtyAtRack.ContainsKey(rack))
            {
                QtyAtRack[rack] += quantity;
            }
            else
            {
                // Add reference
                QtyAtRack.Add(rack, quantity);
                ReservedAtRack.Add(rack, 0); // Initialise reserved quantity to 0

                rack.SKUs.Add(this);
                if (!rack.OnShelf.SKUs.ContainsKey(this))
                    rack.OnShelf.SKUs.Add(this, rack);
            }
        }
        public void PickFromRack(CPRack rack, int quantity = 1)
        {
            if (!QtyAtRack.ContainsKey(rack))
                throw new Exception("SKU not in rack");

            if (QtyAtRack[rack] < quantity)
                throw new Exception("Shortage of item at rack");

            QtyAtRack[rack] -= quantity;
            ReservedAtRack[rack] -= quantity;

            if (QtyAtRack[rack] == 0)
            {
                // Remove reference
                QtyAtRack.Remove(rack);
                ReservedAtRack.Remove(rack);

                rack.SKUs.Remove(this);
                rack.OnShelf.SKUs.Remove(this);
                // Note: SKU remains in layout lookup
            }

        }

        public void ReserveFromRack(CPRack rack, int quantity = 1)
        {
            if (!ReservedAtRack.ContainsKey(rack))
                throw new Exception("SKU not in rack");

            if (GetQtyAvailable(rack) < quantity)
                throw new Exception("Shortage of item at rack for reservation");

            ReservedAtRack[rack] += quantity;
        }
        public int GetQtyAvailable(CPRack rack)
        {
            if (!QtyAtRack.ContainsKey(rack))
                throw new Exception("SKU not in rack");

            return QtyAtRack[rack] - ReservedAtRack[rack];
        }

        public int GetTotalQty()
        {
            return QtyAtRack.Values.Sum();
        }

        #region Zone Implementation
        public HashSet<string> GetFulfilmentZones()
        {
            HashSet<string> output = new HashSet<string>();
            List<CPRack> locations = QtyAtRack.Keys.ToList();

            foreach (var loc in locations)
            {
                if (QtyAtRack[loc] <= 0) throw new Exception("Zero quantity. Should have been removed from Dictionary.");

                if (GetQtyAvailable(loc) > 0)
                    output.Add(loc.GetZone());
            }
            
            return output;
        }

        public bool IsFulfiledZone(string zone)
        {
            return GetFulfilmentZones().Contains(zone);
        }
        #endregion
    }
}
