using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    public class SortingStation
    {
        public OrderBatch orderBatch { get; set; }
        public List<List<PickJob>> picklists { get; set; }
        public bool isAvailable { get; set; }
        public double sortingRate { get; set; } // seconds per item

        public SortingStation()
        {
            orderBatch = null;
            picklists = new List<List<PickJob>>();
            isAvailable = true;

            sortingRate = 5.0; // Need to change to parameter
        }

        public bool IsReadyToSort()
        {
            return orderBatch.PickLists.Count == picklists.Count;
        }

        public void AssignOrderBatch(OrderBatch batch)
        {
            orderBatch = batch;
            isAvailable = false;
        }

        public void CompleteSorting()
        {
            orderBatch = null;
            picklists.Clear();
            isAvailable = true;
        }

        /// <summary>
        /// In minutes. Based on number of items and sorting rate (sec/item)
        /// </summary>
        /// <returns></returns>
        public double GetSortingTime()
        {
            int numItems = picklists.Sum(l => l.Sum(j => j.quantity));

            return 1.0 * sortingRate * numItems / 60.0;
        }
    }
}
