using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Queueing<TLoad> : State<Queueing<TLoad>.Statics>
    {
        #region Static Properties
        public class Statics : Scenario
        {
            /// <summary>
            /// Maximum number of loads in the queue
            /// </summary>
            public int Capacity { get; set; } = int.MaxValue;
        }
        #endregion

        #region Dynamic Properties
        public List<TLoad> Waiting { get; private set; } = new List<TLoad>();
        public int Occupancy { get { return Waiting.Count; } }
        public int Vacancy { get { return Config.Capacity - Occupancy; } }
        public bool ToDequeue { get; private set; } = true;

        public HourCounter HourCounter { get; private set; } = new HourCounter(); // statistics    
        public double Utilization { get { return HourCounter.AverageCount / Config.Capacity; } }
        #endregion

        #region Events
        private abstract class InternalEvent : Event<Queueing<TLoad>, Statics> { }
        /// <summary>
        /// Enqueue given load
        /// </summary>
        private class EnqueueEvent : InternalEvent
        {
            internal TLoad Load { get; set; }
            public override void Invoke()
            {
                if (This.Vacancy == 0) throw new HasZeroVacancyException();
                This.Waiting.Add(Load);
                This.HourCounter.ObserveChange(1, ClockTime);
                Execute(new StateChgEvent());
                if (This.ToDequeue) Execute(new DequeueEvent());                
            }
            public override string ToString() { return string.Format("{0}_Enqueue", This); }
        }        
        private class UpdToDequeueEvent : InternalEvent
        {
            internal bool ToDequeue { get; set; }
            public override void Invoke()
            {
                This.ToDequeue = ToDequeue;
                if (This.ToDequeue && This.Waiting.Count > 0) Execute(new DequeueEvent());
            }
            public override string ToString() { return string.Format("{0}_UpdToDequeue", This); }
        }
        private class StateChgEvent : InternalEvent
        {
            public override void Invoke() { Execute(This.OnStateChg, e => e()); }
            public override string ToString() { return string.Format("{0}_StateChange", This); }
        }
        /// <summary>
        /// Dequeue the first load
        /// </summary>
        private class DequeueEvent : InternalEvent
        {
            public override void Invoke()
            {
                TLoad load = This.Waiting.FirstOrDefault();
                This.Waiting.RemoveAt(0);
                This.HourCounter.ObserveChange(-1, ClockTime);                
                foreach (var evnt in This.OnDequeue) Execute(evnt(load));

                Execute(new StateChgEvent());
                if (This.ToDequeue && This.Waiting.Count > 0) Execute(new DequeueEvent());
            }
            public override string ToString() { return string.Format("{0}_Dequeue", This); }
        }
        #endregion

        #region Input Events - Getters
        public Event Enqueue(TLoad load) { return new EnqueueEvent { This = this, Load = load }; }
        public Event UpdToDequeue(bool toDequeue) { return new UpdToDequeueEvent { This = this, ToDequeue = toDequeue }; }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event>> OnDequeue { get; private set; } = new List<Func<TLoad, Event>>();
        public List<Func<Event>> OnStateChg { get; private set; } = new List<Func<Event>>();
        #endregion

        #region Exeptions
        public class HasZeroVacancyException : Exception
        {
            public HasZeroVacancyException() : base("Make sure the vacancy of the queue is updated before execute Enqueue event.") { }
        }
        #endregion

        public Queueing() : base(new Statics()) { Name = "Queueing"; }
        public Queueing(Statics config, string tag = null) : base(config, tag: tag) { Name = "Queueing"; }

        public override void WarmedUp(DateTime clockTime)
        {
            HourCounter.WarmedUp(clockTime);
        }
        public override void WriteToConsole(DateTime? clockTime = null)
        {
            Console.Write("[{0}]: ", this);
            foreach (var load in Waiting) Console.Write("{0} ", load);
            Console.WriteLine();
        }
    }
}
