namespace O2DESNet.Demos.MM1Queue
{
    internal class StartService : Event
    {
        internal Customer Customer { get; private set; }
        internal StartService(Simulator sim, Customer customer) 
            : base(sim) { Customer = customer; }
        public override void Invoke()
        {
            _sim.Status.Serving = Customer;
            _sim.ScheduleEvent(
                new Departure(_sim, Customer), 
                _sim.Scenario.Generate_ServiceTime(_sim.RS));
        }
    }
}
