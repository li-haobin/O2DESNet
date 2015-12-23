namespace O2DESNet.Template.Events
{
    internal class MyEvent1 : Event<Scenario, Status>
    {
        // include dynamic entities, i.e., loads, involving in the event

        // internal Load_1 load_1 { get; private set; }
        // internal Load_2 load_1 { get; private set; }
        // ...

        protected override void Invoke()
        {
            // updates _sim.Status if necessary

            // schedule subsequent events if necessary
            // Schedule(new MyEvent2 {Load_2 = new Load_2()}, TimeSpan.FromHours(some_random_value));
            // Execute(new MyEvent1 {Load_1 = new Load_1()});
        }
    }
}
