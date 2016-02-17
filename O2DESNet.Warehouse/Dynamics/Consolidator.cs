using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using O2DESNet.Warehouse.Statics;
using O2DESNet.Warehouse.Events;

namespace O2DESNet.Warehouse.Dynamics
{
    public class Consolidator
    {
        public Scenario Scenario { get; private set; }

        public string filename { get; private set; }
        public double sortingRate { get; private set; }

        // TODO: Read this value
        public int NumSortersAvailable { get; set; }
        public Queue<SortingStation> ReadyToSort { get; set; }

        public List<OrderBatch> BatchesToProcess { get; private set; } // What is this for?
        public List<PickList> PickListsArrived { get; private set; } // What is this for?
        public List<SortingStation> AllSortingStations { get; set; }



        public Consolidator(Scenario scenario)
        {
            Scenario = scenario;
            filename = @"Layout\" + scenario.Name + "_Consolidator.csv";
            PickListsArrived = new List<PickList>();
            AllSortingStations = new List<SortingStation>();
            ReadyToSort = new Queue<SortingStation>();

            ReadConsolidatorParameters();
        }

        private void ReadConsolidatorParameters()
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                sr.ReadLine(); // Header

                var line = sr.ReadLine();
                var data = line.Split(',');
                sortingRate = double.Parse(data[1]);
                NumSortersAvailable = int.Parse(data[2]);
            }
        }

        public void ProcessCompletedPicklist(Simulator sim, PickList picklist)
        {
            if (BatchesToProcess == null)
                BatchesToProcess = new List<OrderBatch>(Scenario.OrderBatches);

            var orderBatch = GetOrderBatch(picklist);
            if (orderBatch != null) // To consolidate
            {
                PickListsArrived.Add(picklist);
                var sortingStation = GetSortingStationFor(orderBatch);

                // No sorting station assigned for this orderBatch
                if (sortingStation == null)
                {
                    sortingStation = GetOrCreateAvailableSortingStation();
                    sortingStation.AssignOrderBatch(orderBatch);
                }

                sortingStation.picklists.Add(picklist);

                if (sortingStation.IsReadyToSort())
                {
                    if (NumSortersAvailable > 0)
                    {
                        NumSortersAvailable--;
                        sim.Status.IncrementActiveSorter();

                        sortingStation.CompleteSorting(); // "Virtually" cleared
                        sim.ScheduleEvent(new CompleteSorting(sim, sortingStation), TimeSpan.FromMinutes(sortingStation.GetSortingTime()));
                    }
                    else
                    {
                        ReadyToSort.Enqueue(sortingStation);
                    }
                }
            }
        }


        /// <summary>
        /// Return null if no station handling the orderBatch
        /// </summary>
        /// <param name="orderBatch"></param>
        /// <returns></returns>
        private SortingStation GetSortingStationFor(OrderBatch orderBatch)
        {
            SortingStation station = null;

            foreach (var stn in AllSortingStations)
            {
                if (stn.orderBatch == orderBatch)
                {
                    station = stn;
                    break;
                }
            }

            return station;
        }
        private SortingStation GetOrCreateAvailableSortingStation()
        {
            SortingStation station = null;

            foreach (var stn in AllSortingStations)
            {
                if (stn.isAvailable)
                {
                    station = stn;
                    break;
                }
            }

            // No available sorting station
            if (station == null)
            {
                AllSortingStations.Add(new SortingStation(sortingRate));
                station = AllSortingStations.Last();
            }

            return station;
        }

        /// <summary>
        /// Returns the order batch which the picklist belongs to. Null if non-batched.
        /// </summary>
        /// <param name="picklist"></param>
        /// <returns></returns>
        private OrderBatch GetOrderBatch(PickList picklist)
        {
            OrderBatch orderBatch = null;

            if (Scenario.WhichOrderBatch.ContainsKey(picklist))
                orderBatch = Scenario.WhichOrderBatch[picklist];

            return orderBatch;
        }

    }
}
