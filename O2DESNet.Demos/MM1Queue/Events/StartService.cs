using O2DESNet.Demos.MM1Queue.Dynamics;

namespace O2DESNet.Demos.MM1Queue.Events
{
    internal class StartService : Event<Scenario, Status>
    {
        internal Customer Customer { get; set; }
        public override void Invoke()
        {
            Status.Serving = Customer;
            Schedule(new Departure { Customer = Customer }, Scenario.ServiceTime(DefaultRS));
        }
    }
}
