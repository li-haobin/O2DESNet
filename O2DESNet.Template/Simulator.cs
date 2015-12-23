namespace O2DESNet.Template
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            // schedule the initial event
            // Schedule(new MyEvent1(new Load_1()), TimeSpan.FromHours(some_random_value));
        }
    }
}
