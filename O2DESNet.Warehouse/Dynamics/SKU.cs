using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    public class SKU
    {
        public string SKU_ID { get; private set; }
        public string Description { get; private set; }
        public Dictionary<CPRack, int> QtyAtRack { get; set; } // This makes it dynamic actually...

        public SKU(string sku_id, string description = "")
        {
            SKU_ID = sku_id;
            Description = description;
            QtyAtRack = new Dictionary<CPRack, int>();
        }

        public void AddToRack(CPRack rack, int quantity = 1)
        {
            if (QtyAtRack.ContainsKey(rack))
                QtyAtRack[rack] += quantity;
            else
            {
                // Add reference
                QtyAtRack.Add(rack, quantity);
                rack.SKUs.Add(this);
                rack.OnShelf.SKUs.Add(this, rack);
            }
        }

        public void PickFromRack(CPRack rack, int quantity = 1)
        {
            if (!QtyAtRack.ContainsKey(rack))
                throw new Exception("SKU not in rack");
            else
            {
                if (QtyAtRack[rack] >= quantity)
                    QtyAtRack[rack] -= quantity;
                else
                    throw new Exception("Shortage of item at rack");

                if(QtyAtRack[rack] == 0)
                {
                    // Remove reference
                    QtyAtRack.Remove(rack);
                    rack.SKUs.Remove(this);
                    rack.OnShelf.SKUs.Remove(this);
                    // Note: SKU remains in layout lookup
                }
            }
        }
    }
}
