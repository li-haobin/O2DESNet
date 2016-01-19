using System;
using System.Collections.Generic;

namespace O2DESNet.Demos.Workshop.Statics
{
    public class ProductType
    {
        public int Id { get; set; }
        public double Priority { get; set; }
        public List<WorkStation> JobSequence { get; set; }
        public Func<Random, TimeSpan> InterArrivalTime { get; set; }
        public Func<Random, WorkStation, TimeSpan> ProcessingTime { get; set; }
    }
}
