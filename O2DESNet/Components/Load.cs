using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Load<TScenario, TStatus> : Component
        where TScenario : Scenario
        where TStatus : Status<TScenario>
    {
        #region Statics
        public class StaticPart : Scenario { }
        #endregion

        #region Dynamics
        public List<Tuple<DateTime, Event<TScenario, TStatus>>> TimeStamps { get; private set; }
        public TimeSpan TotalTimeSpan { get { return TimeStamps.Max(t => t.Item1) - TimeStamps.Min(t => t.Item1); } }
        
        public virtual void Log(Event<TScenario, TStatus> evnt)
        {
            TimeStamps.Add(new Tuple<DateTime, Event<TScenario, TStatus>>(evnt.ClockTime, evnt));
            evnt.Log(this, evnt);
        }
        #endregion

        public Load(string tag = null) : base(tag: tag)
        {
            Name = "Load";
            TimeStamps = new List<Tuple<DateTime, Event<TScenario, TStatus>>>();
        }       

        public override void WarmedUp(DateTime clockTime) { }
    }
}
