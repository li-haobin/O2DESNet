using O2DESNet.Demos.Workshop.Dynamics;
using O2DESNet.Demos.Workshop.Statics;
using System;

namespace O2DESNet.Demos.Workshop.Events
{
    internal class Arrive : Event<Scenario, Status>
    {
        public ProductType ProductType { get; set; }
        protected override void Invoke()
        {
            var product = new Product
            {
                Id = Status.ProductsCounter + 1,
                Type = ProductType,
                EnterTime = ClockTime,
                CurrentStage = 0
            };
            Status.ProductsInSystem.Add(product);
            Log("{0}: Product #{1} (Type {2}) arrives.", ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), product.Id, product.Type.Id);

            Status.Queues[product.CurrentWorkStation].Add(product);
            Execute(new AttemptToProcess { WorkStation = product.CurrentWorkStation });

            // schedule the next arrival event
            Schedule(new Arrive { ProductType = ProductType }, ProductType.InterArrivalTime(DefaultRS));
        }
    }
}
