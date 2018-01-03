using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet
{
    public abstract class Event
    {
        private static int _count = 0;
        internal int Index { get; private set; } = _count++;

        internal State State { get; set; } = null;
        internal Simulator Simulator { get; set; }
        internal protected DateTime ClockTime { get { return Simulator.ClockTime; } }
        public abstract void Invoke();
        protected Event() { }
        protected Event(Simulator simulator) { Simulator = simulator; }
        internal DateTime ScheduledTime { get; set; }
        internal protected void Log(string format, params object[] args) { State.Log(ClockTime, string.Format(format, args)); }
        internal protected void Log(params object[] args) { State.Log(ClockTime, args); }
    }
    public abstract class Event<TState, TScenario> : Event 
        where TState : State<TScenario>
        where TScenario : Scenario
    {        
        public TState This { get { return (TState)State; } set { State = value; } }
        protected TScenario Config { get { return This.Config; } }
        protected Random DefaultRS { get { return This.DefaultRS; } }

        protected Event() { }
        public Event(TState state) { State = state; }

        private void Induce(Event evnt) { if (evnt.State == null && State != null) evnt.State = State; }
        /// <summary>
        /// Execute an individual event
        /// </summary>
        /// <param name="evnt">The event to be executed</param>
        protected void Execute(Event evnt) { Induce(evnt); Simulator.Execute(evnt); }
        /// <summary>
        /// Execute events in a batch, described by the list of event getters and a renderer
        /// </summary>
        /// <typeparam name="T">Type of event getters</typeparam>
        /// <param name="batch">Event getters in a batch</param>
        /// <param name="renderer">Renderer for each event</param>
        protected void Execute<T>(List<T> batch, Func<T, Event> renderer)
        {
            foreach (var e in batch) Execute(renderer(e));
        }
        protected void Schedule(Event evnt, DateTime time) { Induce(evnt); Simulator.Schedule(evnt, time); }
        protected void Schedule(Event evnt, TimeSpan delay) { Schedule(evnt, ClockTime + delay); }
        protected void Schedule(Event evnt) { Schedule(evnt, ClockTime); }
    }    
}
