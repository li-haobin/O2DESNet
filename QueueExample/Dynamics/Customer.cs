using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueExample.Dynamics
{
    public class Customer
    {
        private static int _count = 0;
        public int Id { get; private set; }
        public Customer() { Id = _count++; }
        public override string ToString() { return string.Format("Customer#{0}", Id); }
    }
}
