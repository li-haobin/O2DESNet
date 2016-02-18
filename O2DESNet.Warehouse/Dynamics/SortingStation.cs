using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse.Dynamics
{
    /// <summary>
    /// This is essentially a "Virtual" sorting station or buffer
    /// </summary>
    public class SortingStation
    {
        private static int _count = 0;
        public int sortingStationID { get; private set; }

        public OrderBatch orderBatch { get; set; }
        public List<PickList> picklists { get; set; }
        public bool isAvailable { get; set; }
        public double sortingRate { get; set; } // seconds per item
        public int numItems { get; set; }

        public SortingStation(double sortingRate)
        {
            sortingStationID = ++_count;

            orderBatch = null;
            picklists = new List<PickList>();
            isAvailable = true;
            numItems = 0;

            this.sortingRate = sortingRate; // Need to change to parameter
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

        public void ClearSortingStation()
        {
            orderBatch = null;
            picklists.Clear();
            isAvailable = true;
            numItems = 0;
        }

        /// <summary>
        /// In minutes. Based on number of items and sorting rate (sec/item)
        /// </summary>
        /// <returns></returns>
        public double GetSortingTime()
        {
            numItems = GetNumItemsToSort();

            return 1.0 * sortingRate * numItems / 60.0;
        }

        private int GetNumItemsToSort()
        {
            return picklists.Sum(l => l.pickJobs.Sum(j => j.quantity));
        }
    }
}
