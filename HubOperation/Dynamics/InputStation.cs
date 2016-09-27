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
        public int PackagesUnloaded { get; set; }
        public int StationID { get; set; }

        public InputStation(int id, TimeSpan rate)
        {
            isIdle = true;
            StationID = id;
            SvcTime = rate;
            PackagesUnloaded = 0;
        }
    }
}
