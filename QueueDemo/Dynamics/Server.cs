using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueDemo.Dynamics
{
    public class Server
    {
        public bool IsIdle { get; set; }
        public TimeSpan SvcTime { get; set; }

        public Server(TimeSpan rate)
        {
            IsIdle = true;
            SvcTime = rate;
        }
    }
}
