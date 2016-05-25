using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Processor<TLoad> where TLoad : Load
    {
        public int Capacity { get; private set; }
        public HashSet<TLoad> Processing { get; private set; }
        public int Vacancy { get { return Capacity - Processing.Count; } }
        public bool HasVacancy { get { return Vacancy > 0; } }
        public HourCounter HourCounter { get; private set; }

        public Processor(int capacity = 1)
        {
            Capacity = capacity;
            Processing = new HashSet<TLoad>();
            HourCounter = new HourCounter(DateTime.MinValue);
        }

        public void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }

        public void Start(TLoad load, DateTime clockTime)
        {
            if (!HasVacancy) throw new Exception("The Processor does not have vacancy.");
            Processing.Add(load);
            HourCounter.ObserveChange(1, clockTime);
        }

        public void Finish(TLoad load, DateTime clockTime)
        {
            if (!Processing.Contains(load)) throw new Exception("The Processor is not processing the Load.");
            Processing.Remove(load);
            HourCounter.ObserveChange(-1, clockTime);
        }

    }
}
