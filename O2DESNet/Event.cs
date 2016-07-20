using System;

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
        protected DateTime ClockTime { get { return Simulator.ClockTime; } }
        protected Event() { }
        protected Event(Simulator<TScenario, TStatus> simulator) { Simulator = simulator; }
        public abstract void Invoke();
        protected void Execute(Event<TScenario, TStatus> evnt) { evnt.Simulator = Simulator; evnt.Invoke(); }
        protected void Schedule(Event<TScenario, TStatus> evnt, DateTime time) { Simulator.Schedule(evnt, time); }
        protected void Schedule(Event<TScenario, TStatus> evnt, TimeSpan delay) { Simulator.Schedule(evnt, delay); }
        protected void Log(string format, params object[] args) { Status.Log(format, args); }
        public DateTime ScheduledTime { get; set; }
    }
}
