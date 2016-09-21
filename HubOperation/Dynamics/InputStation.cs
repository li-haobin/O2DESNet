using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation.Dynamics
{
    public class InputStation
    {
        public bool isIdle { get; set; }
        public TimeSpan SvcTime { get; set; }

        public InputStation(TimeSpan rate)
        {
            isIdle = true;
            SvcTime = rate;
        }
    }
}
