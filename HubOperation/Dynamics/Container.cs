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
        public DateTime FinishUnloadingTime { get; set; }
        public List<Package> PackagesList { get; set; }
        public bool isEmpty { get; set; }
        public bool isUnloading { get; set; }
        public string Type { get; set; }
        public Container(DateTime ready, List<Package> list)
        {
            ReadyTime = ready;
            PackagesList = new List<Package>(list);
            isEmpty = false;
            isUnloading = false;
            FinishUnloadingTime = new DateTime();
        }

        public Container(Dynamics.Container container)
        {
            ReadyTime = container.ReadyTime;
            PackagesList = new List<Package>(container.PackagesList);
            isEmpty = container.isEmpty;
            isUnloading = container.isUnloading;
            FinishUnloadingTime = container.FinishUnloadingTime;
        }

        public Container()
        {
            ReadyTime = new DateTime();
            PackagesList = new List<Package>();
            isEmpty = false;
            isUnloading = false;
            FinishUnloadingTime = new DateTime();
        }

    }
}
