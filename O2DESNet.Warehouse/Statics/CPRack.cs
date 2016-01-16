using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Statics
{
    [Serializable]
    public class CPRack : ControlPoint
    {
        public string Rack_ID { get; set; }
        public PathShelf OnShelf { get; set; }
        public List<SKU> SKUs { get; set; }

        public CPRack(string rack_ID, PathShelf shelf) : base()
        {
            _count--; //Exclude CPRack from Dijkstra : for performance
            Rack_ID = rack_ID;
            OnShelf = shelf;
            if (shelf == null)
                throw new Exception("Rack must be connected to a shelf");
            else
                OnShelf.Racks.Add(this);

            SKUs = new List<SKU>();
        }

        #region Zone Implementation
        // Currently crude implementation using string

        public string GetZone()
        {
            int idx = Rack_ID.IndexOf('-');

            if (idx > 0) return Rack_ID.Substring(0, idx);
            else return string.Empty;
        }

        public bool IsSameZone(CPRack other)
        {
            return GetZone() == other.GetZone();
        }

        #endregion
    }
}
