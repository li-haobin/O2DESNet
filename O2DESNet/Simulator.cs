using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet
{
    public class Simulator
    {
        internal SortedSet<Event> FutureEventList { get; } = new SortedSet<Event>(new FutureEventComparer());
        public DateTime ClockTime { get; protected set; } = DateTime.MinValue;
        public State State { get; private set; }

        public Simulator(State state)
        {
            #region For Time Dilation
            _realTimeAtDilationReset = ClockTime;
            TimeDilationScale = 1.0;
            #endregion

            State = state;
            //if (State.InitEvents.Count == 0) throw new InitEventsNotFound();
            foreach (var evnt in State.InitEvents) Schedule(evnt, ClockTime);
        }        
        public bool HasFutureEvents { get { return FutureEventList.Count > 0; } }
        public DateTime HeadEventTime { get { return FutureEventList.Count > 0 ? FutureEventList.First().ScheduledTime : DateTime.MaxValue; } }
        /// <summary>
        /// Reset simulation clock time, used for HLA federate initialization.
        /// </summary>
        public void ResetClockTime(DateTime clockTime)
        {
            while (ClockTime < clockTime)
            {
                FutureEventList.Remove(FutureEventList.First());
                ClockTime = FutureEventList.Count > 0 ? FutureEventList.First().ScheduledTime : clockTime;
            }
        }
        internal protected void Schedule(Event evnt, DateTime time)
        {
            if (evnt.Simulator == null) evnt.Simulator = this;
            if (time < ClockTime) throw new Exception("Event cannot be scheduled before ClockTime.");
            evnt.ScheduledTime = time;
            FutureEventList.Add(evnt);
        }
        internal protected void Execute(Event evnt)
        {
            evnt.Simulator = this;
            evnt.Invoke();
        }
        protected bool ExecuteHeadEvent()
        {
            /// pop out the head event from FEL
            var head = FutureEventList.FirstOrDefault();
            if (head == null) return false;
            FutureEventList.Remove(head);

            /// Execute the event
            ClockTime = head.ScheduledTime;
            head.Invoke();
            return true;
        }
        public virtual bool Run(Event evnt)
        {
            if (evnt.Simulator != null && !evnt.Simulator.Equals(this))
                return false;
            Execute(evnt);
            return true;
        }
        public virtual bool Run(TimeSpan duration) { return Run(ClockTime.Add(duration)); }
        public virtual bool Run(DateTime terminate)
        {
            while (true)
            {
                if (FutureEventList.Count < 1) return false; // cannot continue
                if (FutureEventList.First().ScheduledTime <= terminate) ExecuteHeadEvent();
                else
                {
                    ClockTime = terminate;
                    return true; // to be continued
                }
            }
        }
        public virtual bool Run(int eventCount)
        {
            while (eventCount-- > 0)
                if (!ExecuteHeadEvent()) return false;
            return true;
        }
        private DateTime? _realTimeForLastRun = null;
        public virtual bool Run(double speed)
        {
            var rtn = true;
            if (_realTimeForLastRun != null)
                rtn = Run(terminate: ClockTime.AddSeconds((DateTime.Now - _realTimeForLastRun.Value).TotalSeconds * speed));
            _realTimeForLastRun = DateTime.Now;
            return rtn;
        }
        public bool WarmUp(TimeSpan duration)
        {
            var r = Run(duration);
            State.WarmedUp(ClockTime);
            return r; // to be continued
        }
        public void WriteToConsole() { State.WriteToConsole(ClockTime); }

        #region For Time Dilation
        private DateTime _realTimeAtDilationReset;
        private DateTime _dilatedTimeAtDilationScaleReset;
        private double _timeDilattionScale;
        public double TimeDilationScale
        {
            get { return _timeDilattionScale; }
            set
            {
                _dilatedTimeAtDilationScaleReset = DilatedClock;
                _realTimeAtDilationReset = ClockTime;
                _timeDilattionScale = value;
            }
        }
        public DateTime DilatedClock
        {
            get { return GetDilatedTime(ClockTime); }
            private set { ClockTime = GetRealTime(value); }
        }
        private DateTime GetDilatedTime(DateTime realTime)
        {
            return _dilatedTimeAtDilationScaleReset +
                TimeSpan.FromSeconds((realTime - _realTimeAtDilationReset).TotalSeconds * TimeDilationScale);
        }
        private DateTime GetRealTime(DateTime dilatedTime)
        {
            return _realTimeAtDilationReset +
                TimeSpan.FromSeconds((dilatedTime - _dilatedTimeAtDilationScaleReset).TotalSeconds / TimeDilationScale);
        }
        private DateTime DilatedScheduledTimeForHeadEvent { get { return GetDilatedTime(FutureEventList.First().ScheduledTime); } }

        static private bool ExecuteHeadEvent_withTimeDilation(Simulator[] simulations)
        {
            var toExecute = simulations.Where(s => s.FutureEventList.Count > 0)
                .OrderBy(s => s.DilatedScheduledTimeForHeadEvent).FirstOrDefault();
            if (toExecute != null)
            {
                var result = toExecute.ExecuteHeadEvent();
                foreach (var s in simulations) if (s != toExecute) s.DilatedClock = toExecute.DilatedClock; //set common clock
                return result;
            }
            return false;
        }
        static public void Run_withTimeDilation(Simulator[] simulations, int eventCount)
        {
            while (eventCount > 0 && ExecuteHeadEvent_withTimeDilation(simulations)) eventCount--;
        }

        #endregion   

        public class InitEventsNotFound : Exception
        {
            public InitEventsNotFound() : base("The Assembly Component must have at least one initial events.") { }
        }        
    }

    public class FutureEventComparer : IComparer<Event>
    {
        public int Compare(Event x, Event y)
        {
            int compare = x.ScheduledTime.CompareTo(y.ScheduledTime);
            if (compare == 0) return x.Index.CompareTo(y.Index);
            return compare;
        }
    }
}
