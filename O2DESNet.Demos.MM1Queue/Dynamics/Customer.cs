using System;

namespace O2DESNet.Demos.MM1Queue
{
    internal class Customer
    {
        private static int _count = 0;
        public int Id { get; private set; }
        public DateTime ArrivalTime { get; internal set; }
        public DateTime DepartureTime { get; internal set; }
        public TimeSpan InSystemDuration { get { return DepartureTime - ArrivalTime; } }
        public Customer() { Id = ++_count; }
        public override string ToString() { return string.Format("Customer #{0}", Id); }
    }
}
