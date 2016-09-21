using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Dynamics
{
    public class DeliveryVan
    {
        public List<Package> PackagesDeliveryList { get; set; }

        public DeliveryVan(List<Package> list )
        {
            PackagesDeliveryList = list;
        }

    }
}
