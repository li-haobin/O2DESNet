using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueDemo.Dynamics
{
    public class Queue
    {
        public List<DateTime> TimeStamp { get; set; }
        public int Length { get; set; }

        public Queue()
        {
            TimeStamp = new List<DateTime>();
            Length = 0;
        }
    }
}
