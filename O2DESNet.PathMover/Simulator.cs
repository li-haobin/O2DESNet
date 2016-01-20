using O2DESNet.PathMover.Events;

namespace O2DESNet.PathMover
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status):base(status)
        {
            Schedule(new Start(), ClockTime);
        }
    }
}
