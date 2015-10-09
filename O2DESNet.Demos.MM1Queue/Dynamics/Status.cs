using System.Collections.Generic;

namespace O2DESNet.Demos.MM1Queue
{
    internal class Status
    {
        private Simulator _sim;
        public Queue<Customer> WaitingQueue { get; internal set; }
        public Customer Serving { get; internal set; }
        public List<Customer> ServedCustomers { get; private set; }
        public HourCounter InSystemCounter { get; internal set; }

        internal Status(Simulator simulation)
        {
            _sim = simulation;
            WaitingQueue = new Queue<Customer>();
            Serving = null;
            InSystemCounter = new HourCounter(_sim);
            ServedCustomers = new List<Customer>();
        }
        internal void Arrive(Customer customer)
        {
            customer.ArrivalTime = _sim.ClockTime;
            InSystemCounter.ObserveChange(1);
        }
        internal void Depart(Customer customer)
        {
            customer.DepartureTime = _sim.ClockTime;
            ServedCustomers.Add(customer);
            InSystemCounter.ObserveChange(-1);
        }
    }
}
