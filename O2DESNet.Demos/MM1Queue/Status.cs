using O2DESNet.Demos.MM1Queue.Dynamics;
using System;
using System.Collections.Generic;

namespace O2DESNet.Demos.MM1Queue
{
    public class Status : Status<Scenario>
    {
        public System.Collections.Generic.Queue<Customer> WaitingQueue { get; internal set; }
        public Customer Serving { get; internal set; }
        public List<Customer> ServedCustomers { get; private set; }
        public HourCounter InSystemCounter { get; internal set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            WaitingQueue = new System.Collections.Generic.Queue<Customer>();
            Serving = null;
            InSystemCounter = new HourCounter(DateTime.MinValue);
            ServedCustomers = new List<Customer>();
        }
        internal void LogArrival(Customer customer, DateTime timestamp)
        {
            customer.ArrivalTime = timestamp;
            InSystemCounter.ObserveChange(1, timestamp);
        }
        internal void LogDeparture(Customer customer, DateTime timestamp)
        {
            customer.DepartureTime = timestamp;
            ServedCustomers.Add(customer);
            InSystemCounter.ObserveChange(-1, timestamp);
        }
    }
}
