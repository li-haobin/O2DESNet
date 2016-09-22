﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Dynamics
{
    public class DeliveryVan
    {
        public List<Package> PackagesDeliveryList { get; set; }
        public List<Package> PackagesLoaded { get; set; }

        public bool isFullyLoaded { get; set; }

        public string Route { get; }
        public DateTime DeliveryReadyTime { get; set; }

        public DeliveryVan(List<Package> list, string route)
        {
            PackagesDeliveryList = list;
            PackagesLoaded = new List<Package>();
            Route = route;
            isFullyLoaded = false;
            DeliveryReadyTime = new DateTime();
        }

        public DeliveryVan(DeliveryVan van)
        {
            PackagesDeliveryList = van.PackagesDeliveryList;
            PackagesLoaded = van.PackagesLoaded;
            Route = van.Route;
            isFullyLoaded = van.isFullyLoaded;
            DeliveryReadyTime = van.DeliveryReadyTime;
        }

        public bool CheckIfFullyLoaded()
        {
            if (PackagesDeliveryList.Count == PackagesLoaded.Count)
            {
                isFullyLoaded = true;
            }

            return isFullyLoaded;
        }
    }
}
