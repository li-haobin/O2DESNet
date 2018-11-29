using O2DESNet.Animation;
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
        protected virtual void Execute(Event evnt) { Simulator.Execute(evnt); }
        internal protected IAnimator Animator { get { return Simulator.Animator; } }
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
        protected override void Execute(Event evnt) { Induce(evnt); Simulator.Execute(evnt); }
        /// <summary>
        /// Execute events in a batch
        /// </summary>
        /// <param name="events">Batch of events to be executed</param>
        protected void Execute(IEnumerable<Event> events)
        {
            foreach (var e in events.ToList()) Execute(e);
        }
        protected void Schedule(Event evnt, DateTime time) { Induce(evnt); Simulator.Schedule(evnt, time); }
        protected void Schedule(Event evnt, TimeSpan delay) { Schedule(evnt, ClockTime + delay); }
        protected void Schedule(Event evnt) { Schedule(evnt, ClockTime); }
        protected void Schedule(IEnumerable<Event> events)
        {
            foreach (var e in events) Schedule(e);
        }
    }    
}
