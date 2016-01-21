﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Dynamics;

namespace O2DESNet.Warehouse.Events
{
    [Serializable]
    internal class CompleteSorting : Event
    {
        internal SortingStation sortingStation;

        internal CompleteSorting(Simulator sim, SortingStation station) : base(sim)
        {
            sortingStation = station;
        }


        public override void Invoke()
        {
            int numItems = sortingStation.numItems;
            _sim.Status.NumItemsSorted += numItems;
            if (numItems > _sim.Status.MaxNumItemsSorted) _sim.Status.MaxNumItemsSorted = numItems;

            sortingStation.CompleteSorting();
        }

        public override void Backtrack()
        {
            throw new NotImplementedException();
        }
    }
}
