using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Dynamics
{
    public class Package
    {
        public string RouteID { get; set; }

        public Package(string route)
        {
            RouteID = route;
        }

        public Package(Package package)
        {
            RouteID = package.RouteID;
        }
    }
}
