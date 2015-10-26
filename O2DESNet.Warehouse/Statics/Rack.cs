using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.Statics
{
    public class CPRack : ControlPoint
    {
        public List<SKU> SKUs { get; set; }

        public CPRack() : base()
        {
            InitializeRack();
        }

        public void InitializeRack()
        {
            SKUs = new List<SKU>();
        }
    }
}
