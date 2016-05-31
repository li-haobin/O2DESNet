using O2DESNet.Demos.MM1Queue.Dynamics;
using O2DESNet.Demos.MM1Queue.Events;

namespace O2DESNet.Demos.MM1Queue
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            // schedule the initial event
            Schedule(new Arrival { Customer = new Customer() }, Scenario.InterArrivalTime(DefaultRS));
        }
    }
}
