using O2DESNet.Demos.MM1Queue.Dynamics;

namespace O2DESNet.Demos.MM1Queue.Events
{
    internal class Departure : Event<Scenario, Status>
    {
        internal Customer Customer { get; set; }
        protected override void Invoke()
        {
            Status.LogDeparture(Customer, Simulator.ClockTime);
            if (Status.WaitingQueue.Count > 0) Execute(new StartService { Customer = Status.WaitingQueue.Dequeue() });
            else Status.Serving = null;
        }
    }
}
