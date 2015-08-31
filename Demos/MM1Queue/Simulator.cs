using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.MM1Queue
{
    class Simulator : O2DESNet.Simulator
    {
        private Random _rs;
        private TimeSpan _expectedInterArrivalTime;
        private TimeSpan _expectedServiceTime;
        private Queue<Customer> _waitingQueue;
        private Customer _servingCustomer = null;
        public bool Debug = false;
        public EventAnalyzer<Customer> CustomerEventRecorder { get; set; }
        
        public Simulator(TimeSpan expectedInterArrivalTime, TimeSpan expectedServiceTime, int seed)
        {
            _expectedInterArrivalTime = expectedInterArrivalTime;
            _expectedServiceTime = expectedServiceTime;
            _waitingQueue = new Queue<Customer>();
            _rs = new Random(seed);
            CustomerEventRecorder = new EventAnalyzer<Customer>(this);
            // an initial event
            ScheduleCustomerArrival();
        }
        private void ScheduleCustomerArrival()
        {
            ScheduleEvent(Arrival(new Customer()), ClockTime + RandomTime.Exponential(_expectedInterArrivalTime, _rs));
        }
        private Event Arrival(Customer customer)
        {
            return delegate()
            {
                CustomerEventRecorder.CheckIn(customer, "Arrival", Debug);
                if (_servingCustomer == null) StartService(customer)();
                else _waitingQueue.Enqueue(customer);
                ScheduleCustomerArrival();
            };
        }
        private Event Process(Customer customer)
        {
            return delegate()
            {
                if (_servingCustomer == null) StartService(customer)();
                else _waitingQueue.Enqueue(customer);
            };
        }
        private Event StartService(Customer customer)
        {
            return delegate()
            {
                _servingCustomer = customer;
                CustomerEventRecorder.CheckIn(customer, "ServiceStart", Debug);
                ScheduleEvent(Depart(customer), ClockTime + RandomTime.Exponential(_expectedServiceTime, _rs));
            };
        }
        private Event Depart(Customer customer)
        {
            return delegate()
            {
                CustomerEventRecorder.CheckIn(customer, "Departure", Debug);
                if (_waitingQueue.Count > 0) StartService(_waitingQueue.Dequeue())();
                else _servingCustomer = null;                
            };
        }
    }

    class Customer
    {
        private static int _count = 0;
        public int Id { get; private set; }
        public Customer() { Id = ++_count; }
        public override string ToString() { return string.Format("Customer #{0}", Id); }
    }
}
