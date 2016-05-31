using O2DESNet.Demos.Workshop.Dynamics;
using O2DESNet.Demos.Workshop.Statics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Demos.Workshop
{
    public class Status:Status<Scenario>
    {
        public List<Machine> Machines { get; private set; }
        public List<Product> ProductsInSystem { get; private set; }
        public List<Product> ProductsDeparted { get; private set; }
        public int ProductsCounter { get { return ProductsInSystem.Count + ProductsDeparted.Count; } }
        public Dictionary<WorkStation, List<Product>> Queues { get; private set; }
        public List<double> TimeSeries_ProductHoursInSystem { get; private set; }

        internal Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            Machines = Scenario.WorkStations.SelectMany(ws => Enumerable.Range(0, ws.N_Machines)
                .Select(i => new Machine { WorkStation = ws, Processing = null })).ToList();
            Queues = Scenario.WorkStations.ToDictionary(s => s, s => new List<Product>());
            for (int i = 0; i < Machines.Count; i++) Machines[i].Id = i + 1;
            ProductsInSystem = new List<Product>();
            ProductsDeparted = new List<Product>();
            TimeSeries_ProductHoursInSystem = new List<double>();
        }
        
        internal void LogDeparture(Product departing, DateTime timestamp)
        {
            departing.ExitTime = timestamp;
            ProductsDeparted.Add(departing);
            ProductsInSystem.Remove(departing);
            TimeSeries_ProductHoursInSystem.Add((departing.ExitTime - departing.EnterTime).TotalHours);
        }
    }
    

}
