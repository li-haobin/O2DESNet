using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Load<TScenario, TStatus>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
    {
        private static int _count = 0;
        public int Id { get; protected set; }
        public List<Tuple<DateTime, Event<TScenario, TStatus>>> TimeStamps { get; private set; }
        public TimeSpan TotalTimeSpan { get { return TimeStamps.Max(t => t.Item1) - TimeStamps.Min(t => t.Item1); } }
        public Load()
        {
            Id = _count++;
            TimeStamps = new List<Tuple<DateTime, Event<TScenario, TStatus>>>();
        }
        public override string ToString() { return string.Format("Load#{0}", Id); }
        public virtual void Log(Event<TScenario, TStatus> evnt)
        {
            TimeStamps.Add(new Tuple<DateTime, Event<TScenario, TStatus>>(evnt.ClockTime, evnt));
            evnt.Log(this, evnt);
        }
    }
}
