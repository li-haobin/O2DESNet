using O2DESNet.Demos.Workshop.Events;

namespace O2DESNet.Demos.Workshop
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            // schedule the initial events
            foreach (var productType in Scenario.ProductTypes)
                Schedule(new Arrive { ProductType = productType }, productType.InterArrivalTime(DefaultRS));
        }
    }
}
