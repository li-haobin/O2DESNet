using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSimulator
{
    public delegate void Event();
    
    public class DESModel
    {      
        private List<FutureEvent> _futureEventList;
        public DateTime ClockTime { get; protected set; }
        
        public DESModel()
        {
            ClockTime = DateTime.MinValue;
            _futureEventList = new List<FutureEvent>();
            
            #region For Time Dilation
            _realTimeAtDilationReset = ClockTime;
            TimeDilationScale = 1.0;            
            #endregion
        }

        protected void ScheduleEvent(Event evnt, TimeSpan delay) { ScheduleEvent(evnt, ClockTime + delay); }
        protected void ScheduleEvent(Event evnt, DateTime time)
        {
            _futureEventList.Add(new FutureEvent { ScheduledTime = time, Event = evnt });
            _futureEventList.Sort(delegate(FutureEvent x, FutureEvent y)
            {
                return x.ScheduledTime.CompareTo(y.ScheduledTime);
            });
        }

        protected bool ExecuteHeadEvent()
        {
            /// pop out the head event from FEL
            var head = _futureEventList.FirstOrDefault();
            if (head == null) return false;
            _futureEventList.RemoveAt(0);

            /// Execute the event
            ClockTime = head.ScheduledTime;
            head.Event();
            return true;
        }
        public virtual bool Run(TimeSpan duration)
        {
            var TimeTerminate = ClockTime.Add(duration);
            while (_futureEventList.First().ScheduledTime <= TimeTerminate)
                if (!ExecuteHeadEvent()) return false;
            return true;
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
                //Console.WriteLine("Scale Reset @ RealClock:{0} DilatedClock:{1}", ClockTime, DilatedClock);
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
        private DateTime DilatedScheduledTimeForHeadEvent { get { return GetDilatedTime(_futureEventList.First().ScheduledTime); } }

        static private bool ExecuteHeadEvent_withTimeDilation(DESModel[] simulations)
        {
            var toExecute = simulations.Where(s => s._futureEventList.Count > 0)
                .OrderBy(s => s.DilatedScheduledTimeForHeadEvent).FirstOrDefault();
            if (toExecute != null)
            {
                Console.WriteLine("*** Simulation #{0} is Executed! ***", simulations.ToList().IndexOf(toExecute) + 1);
                var result = toExecute.ExecuteHeadEvent();
                foreach (var s in simulations) if (s != toExecute) s.DilatedClock = toExecute.DilatedClock; //set common clock
                return result;
            }
            return false;
        }
        static public void Run_withTimeDilation(DESModel[] simulations, int eventCount)
        {
            while (eventCount > 0 && ExecuteHeadEvent_withTimeDilation(simulations)) eventCount--;
            if (true) // for debug
            {
                Console.WriteLine();
                foreach (var sim in simulations)
                    Console.WriteLine("Sim #{0} - Dilated Clock: {1}, Dilated Time for Head Event: {2}",
                        simulations.ToList().IndexOf(sim), sim.DilatedClock, sim.DilatedScheduledTimeForHeadEvent);
            }
        }
        #endregion
    }

    internal class FutureEvent
    {
        public DateTime ScheduledTime { get; set; }
        public Event Event { get; set; }
    }
}
