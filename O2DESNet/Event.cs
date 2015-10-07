using System;

namespace O2DESNet
{
    public abstract class Event
    {
        protected Simulator _simulator;
        protected Event(Simulator simulator) { _simulator = simulator; }
        public virtual Action Invoke { get; protected set; }
        protected void ScheduleEvent(Event evnt, TimeSpan delay) { ScheduleEvent(evnt, _simulator.ClockTime + delay); }
        protected void ScheduleEvent(Event evnt, DateTime time)
        {
            _simulator.FutureEventList.Add(new FutureEvent { ScheduledTime = time, Event = evnt });
            _simulator.FutureEventList.Sort(delegate (FutureEvent x, FutureEvent y)
            {
                return x.ScheduledTime.CompareTo(y.ScheduledTime);
            });
        }
    }
}
