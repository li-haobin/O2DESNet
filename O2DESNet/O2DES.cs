using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet
{
    public abstract class O2DES
    {
        internal List<FutureEvent> FutureEventList;
        public DateTime ClockTime { get; protected set; }

        public O2DES()
        {
            ClockTime = DateTime.MinValue;
            FutureEventList = new List<FutureEvent>();

            #region For Time Dilation
            _realTimeAtDilationReset = ClockTime;
            TimeDilationScale = 1.0;
            #endregion
        }
        public void ScheduleEvent(IEvent evnt, TimeSpan delay) { ScheduleEvent(evnt, ClockTime + delay); }
        public void ScheduleEvent(IEvent evnt, DateTime time)
        {
            FutureEventList.Add(new FutureEvent { ScheduledTime = time, Event = evnt });
            FutureEventList.Sort(delegate (FutureEvent x, FutureEvent y)
            {
                return x.ScheduledTime.CompareTo(y.ScheduledTime);
            });
        }
        protected bool ExecuteHeadEvent()
        {
            /// pop out the head event from FEL
            var head = FutureEventList.FirstOrDefault();
            if (head == null) return false;
            FutureEventList.RemoveAt(0);

            /// Execute the event
            ClockTime = head.ScheduledTime;
            head.Event.Invoke();
            return true;
        }
        public virtual bool Run(TimeSpan duration)
        {
            var TimeTerminate = ClockTime.Add(duration);
            while (true)
            {
                if (FutureEventList.Count < 1) return false; // cannot continue
                if (FutureEventList.First().ScheduledTime <= TimeTerminate) ExecuteHeadEvent();
                else return true; // to be continued
            }
        }
        public virtual bool Run(int eventCount)
        {
            while (eventCount-- > 0)
                if (!ExecuteHeadEvent()) return false;
            return true;
        }

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

        static private bool ExecuteHeadEvent_withTimeDilation(O2DES[] simulations)
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
        static public void Run_withTimeDilation(O2DES[] simulations, int eventCount)
        {
            while (eventCount > 0 && ExecuteHeadEvent_withTimeDilation(simulations)) eventCount--;
        }
        #endregion
    }

    internal class FutureEvent
    {
        public DateTime ScheduledTime { get; set; }
        public IEvent Event { get; set; }
    }

    public interface IEvent { void Invoke(); }
}
