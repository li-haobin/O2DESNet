using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet
{
    public abstract class Event<TScenario, TState>
        where TScenario : Scenario
        where TState : State<TScenario>
    {
        private static int _count = 0;
        internal int Index { get; private set; } = _count++;
        internal protected Simulator<TScenario, TState> Simulator { get; set; }
        protected TState State { get { return Simulator.State; } }
        protected TScenario Scenario { get { return State.Scenario; } }
        protected Random DefaultRS { get { return State.DefaultRS; } }
        internal protected DateTime ClockTime { get { return Simulator.ClockTime; } }
        protected Event() { }
        protected Event(Simulator<TScenario, TState> simulator) { Simulator = simulator; }
        public abstract void Invoke();
        protected void Execute(Event<TScenario, TState> evnt) { evnt.Simulator = Simulator; evnt.Invoke(); }
        protected void Schedule(Event<TScenario, TState> evnt, DateTime time) { Simulator.Schedule(evnt, time); }
        protected void Schedule(Event<TScenario, TState> evnt, TimeSpan delay) { Simulator.Schedule(evnt, delay); }
        internal protected void Log(string format, params object[] args) { State.Log(ClockTime, string.Format(format, args)); }
        internal protected void Log(params object[] args) { State.Log(ClockTime, args); }
        public DateTime ScheduledTime { get; set; }
    }

    /// <summary>
    /// A generic event using default Scenario and Status class
    /// </summary>
    public abstract class Event : Event<Scenario, State<Scenario>> { }
}
