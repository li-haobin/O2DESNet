using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Dynamics
{

    /// <summary>
    /// Contains packages
    /// 
    /// </summary>
    public class Container
    {
        public DateTime ReadyTime { get; set; }
        public List<Package> PackagesList { get; set; }
        public int PackagesCount { get; set; }
        public bool isEmpty { get; set; }
        public bool isUnloading { get; set; }
        public Container(DateTime ready, List<Package> list)
        {
            ReadyTime = ready;
            PackagesList = new List<Package>(list);
            isEmpty = false;
            isUnloading = false;
            PackagesCount = PackagesList.Count();
        }
    }
}
