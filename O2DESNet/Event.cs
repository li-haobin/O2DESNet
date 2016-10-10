using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet
{
    public abstract class Event<TScenario, TStatus>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
    {
        internal protected Simulator<TScenario, TStatus> Simulator { get; set; }
        protected TStatus Status { get { return Simulator.Status; } }
        protected TScenario Scenario { get { return Status.Scenario; } }
        protected Random DefaultRS { get { return Status.DefaultRS; } }
        internal protected DateTime ClockTime { get { return Simulator.ClockTime; } }
        protected Event() { }
        protected Event(Simulator<TScenario, TStatus> simulator) { Simulator = simulator; }
        public abstract void Invoke();
        protected void Execute(Event<TScenario, TStatus> evnt) { evnt.Simulator = Simulator; evnt.Invoke(); }
        protected void Schedule(Event<TScenario, TStatus> evnt, DateTime time) { Simulator.Schedule(evnt, time); }
        protected void Schedule(Event<TScenario, TStatus> evnt, TimeSpan delay) { Simulator.Schedule(evnt, delay); }
        internal protected void Log(string format, params object[] args) { Status.Log(format, args); }
        internal protected void Log(params object[] args) { Status.Log(new object[] { ClockTime }.Concat(args).ToArray()); }
        public DateTime ScheduledTime { get; set; }
    }

    /// <summary>
    /// A generic event using default Scenario and Status class
    /// </summary>
    public abstract class Event : Event<Scenario, Status<Scenario>> { }
}
