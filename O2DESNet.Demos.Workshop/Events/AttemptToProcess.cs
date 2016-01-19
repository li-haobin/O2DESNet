using O2DESNet.Demos.Workshop.Dynamics;
using O2DESNet.Demos.Workshop.Statics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Demos.Workshop.Events
{
    internal class AttemptToProcess : Event<Scenario, Status>
    {
        internal WorkStation WorkStation { get; set; }
        protected override void Invoke()
        {
            var machine = Status.Machines.Where(m => m.WorkStation == WorkStation && m.IsIdle).FirstOrDefault();
            if (machine != null)
            {
                // pull the same type of waiting products from queue, according to priority, and machine capacity
                var products = new List<Product>();
                var queue = Status.Queues[WorkStation];
                queue.Sort((p1, p2) => p1.Type.Priority.CompareTo(p2.Type.Priority));
                while (queue.Count > 0 && products.Count < Scenario.MachineCapacity)
                {
                    Product product;
                    if (products.Count < 1) product = queue.First();
                    else product = queue.FirstOrDefault(p => p.Type == products.Last().Type);
                    if (product == null) break;
                    products.Add(product);
                    queue.Remove(product);
                }
                // if there are waiting products, process them
                if (products.Count > 0)
                {
                    machine.Processing = products;
                    foreach (var p in products)
                    {
                        p.BeingProcessedBy = machine;
                        Log("{0}: Product #{1} (Type {2}) @ WS #{3} starts process.", ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), p.Id, p.Type.Id, p.CurrentWorkStation.Id);
                    }
                    Schedule(new FinishProcess { Machine = machine }, products.First().Type.ProcessingTime(DefaultRS, WorkStation));
                }
            }
        }
    }
}
