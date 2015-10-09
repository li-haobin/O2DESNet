namespace O2DESNet.Demos.MM1Queue
{
    internal class Departure : Event
    {
        internal Customer Customer { get; private set; }
        internal Departure(Simulator sim, Customer customer) 
            : base(sim) { Customer = customer; }
        public override void Invoke()
        {
            _sim.Status.Depart(Customer);
            if (_sim.Status.WaitingQueue.Count > 0)
                new StartService(_sim, _sim.Status.WaitingQueue.Dequeue()).Invoke();
            else _sim.Status.Serving = null;
        }
    }
}
