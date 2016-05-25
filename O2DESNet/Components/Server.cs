using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Components
{
    public class Server<TLoad>
    {
        public int Capacity { get; private set; }
        public HashSet<TLoad> Processing { get; private set; }
        public int Vancancy { get { return Capacity - Processing.Count; } }
        public bool IsIdle { get { return Vancancy > 0; } }
        public HourCounter HourCounter { get; private set; }

        public Server(int capacity)
        {
            Capacity = capacity;
            Processing = new HashSet<TLoad>();
            HourCounter = new HourCounter(DateTime.MinValue);
        }

        public void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }

        public bool Start(TLoad load, DateTime clockTime)
        {
            if (!IsIdle) return false;
            Processing.Add(load);
            HourCounter.ObserveChange(1, clockTime);
            return true;
        }

        public bool Finish(TLoad load, DateTime clockTime)
        {
            if (!Processing.Contains(load)) return false;
            Processing.Remove(load);
            HourCounter.ObserveChange(-1, clockTime);
            return true;
        }

    }
}
