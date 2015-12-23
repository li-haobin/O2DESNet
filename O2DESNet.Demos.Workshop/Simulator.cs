using O2DESNet.Demos.Workshop.Events;

namespace O2DESNet.Demos.Workshop
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            // schedule the initial event
            Schedule(new Arrival(), Scenario.Generate_InterArrivalTime(DefaultRS));
        }
    }
}
