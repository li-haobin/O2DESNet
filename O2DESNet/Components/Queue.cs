using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Queue<TScenario, TStatus, TLoad> : Component
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Static Properties
        public class StaticProperties : Scenario
        {
            /// <summary>
            /// Maximum number of loads in the queue
            /// </summary>
            public int Capacity { get; set; } = int.MaxValue;
            /// <summary>
            /// Dequeuing condition for each load
            /// </summary>
            public Func<TLoad, bool> ToDequeue { get; set; }
        }
        public StaticProperties Statics { get; private set; }
        #endregion

        #region Dynamic Properties
        public List<TLoad> Waiting { get; private set; }
        public int Vancancy { get { return Statics.Capacity - Waiting.Count; } }

        public HourCounter HourCounter { get; private set; } // statistics    
        public double Utilization { get { return HourCounter.AverageCount / Statics.Capacity; } }    
        #endregion

        #region Events
        /// <summary>
        /// Enqueue given load
        /// </summary>
        private class EnqueueEvent : Event<TScenario, TStatus>
        {
            public Queue<TScenario, TStatus, TLoad> Queue { get; private set; }
            public TLoad Load { get; private set; }

            internal EnqueueEvent(Queue<TScenario, TStatus, TLoad> queue, TLoad load)
            {
                Queue = queue;
                Load = load;
            }
            public override void Invoke()
            {
                if (Queue.Vancancy == 0) throw new HasZeroVacancyException();
                Load.Log(this);
                Queue.Waiting.Add(Load);
                Queue.HourCounter.ObserveChange(1, ClockTime);
                Execute(Queue.Dequeue());
            }
            public override string ToString() { return string.Format("{0}_Enqueue", Queue); }
        }
        /// <summary>
        /// Attempt to dequeue the first load
        /// </summary>
        private class DequeueEvent : Event<TScenario, TStatus>
        {
            public Queue<TScenario, TStatus, TLoad> Queue { get; private set; }

            internal DequeueEvent(Queue<TScenario, TStatus, TLoad> queue)
            {
                Queue = queue;
            }
            public override void Invoke()
            {
                if (Queue.Statics.ToDequeue == null) throw new DequeueConditionNotSpecifiedException();
                var load = Queue.Waiting.FirstOrDefault();
                if (load == null) return;
                if (Queue.Statics.ToDequeue(load))
                {
                    load.Log(this);
                    Queue.Waiting.RemoveAt(0);
                    Queue.HourCounter.ObserveChange(-1, ClockTime);
                    foreach (var evnt in Queue.OnDequeue) Execute(evnt(load));
                }
            }
            public override string ToString() { return string.Format("{0}_Dequeue", Queue); }
        }
        #endregion

        #region Input Events - Getters
        public Event<TScenario, TStatus> Enqueue(TLoad load) { return new EnqueueEvent(this, load); }
        public Event<TScenario, TStatus> Dequeue() { return new DequeueEvent(this); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnDequeue { get; private set; }
        #endregion

        #region Exeptions
        public class HasZeroVacancyException : Exception
        {
            public HasZeroVacancyException() : base("Check vacancy of the queue before execute Enqueue event.") { }
        }
        public class DequeueConditionNotSpecifiedException : Exception
        {
            public DequeueConditionNotSpecifiedException() : base("Set ToDequeue for the dequeue condition.") { }
        }
        #endregion

        public Queue(StaticProperties statics, string tag = null) : base(tag: tag)
        {
            Name = "Queue";
            Statics = statics;
            Waiting = new List<TLoad>();
            HourCounter = new HourCounter();

            // initialize for output events
            OnDequeue = new List<Func<TLoad, Event<TScenario, TStatus>>>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            HourCounter.WarmedUp(clockTime);
        }
        public override void WriteToConsole()
        {
            Console.Write("[{0}]: ", this);
            foreach (var load in Waiting) Console.Write("{0} ", load);
            Console.WriteLine();
        }
    }
}
