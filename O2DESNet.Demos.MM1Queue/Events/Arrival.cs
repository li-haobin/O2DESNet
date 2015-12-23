using O2DESNet.Demos.MM1Queue.Dynamics;

namespace O2DESNet.Demos.MM1Queue.Events
{
    internal class Arrival : Event<Scenario, Status>
    {
        internal Customer Customer { get; set; }
        protected override void Invoke()
        {
            Status.LogArrival(Customer, ClockTime);
            if (Status.Serving == null) Execute(new StartService { Customer = Customer });
            else Status.WaitingQueue.Enqueue(Customer);
            Schedule(new Arrival { Customer = new Customer() }, Scenario.InterArrivalTime(DefaultRS));
        }
    }
}
