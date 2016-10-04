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
        public DateTime? GetFirstTimeStamp(Func<Event<TScenario, TStatus>, bool> check = null)
        {
            for (int i = 0; i < TimeStamps.Count; i++)
                if (check == null || check(TimeStamps[i].Item2)) return TimeStamps[i].Item1;
            return null;
        }
        public DateTime? GetLastTimeStamp(Func<Event<TScenario, TStatus>,bool> check)
        {
            for (int i = TimeStamps.Count; i > 0; i--)
                if (check == null || check(TimeStamps[i - 1].Item2)) return TimeStamps[i - 1].Item1;
            return null;
        }

        public virtual void Log(Event<TScenario, TStatus> evnt)
        {
            TimeStamps.Add(new Tuple<DateTime, Event<TScenario, TStatus>>(evnt.ClockTime, evnt));
            evnt.Log(this, evnt);
        }
        #endregion

        public Load(int seed = 0, string tag = null) : base(seed: seed, tag: tag)
        {
            Name = "Load";
            TimeStamps = new List<Tuple<DateTime, Event<TScenario, TStatus>>>();
        }       

        public override void WarmedUp(DateTime clockTime) { }
    }
}
