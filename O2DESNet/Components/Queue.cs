using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Queue<TScenario, TStatus, TLoad>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Static Properties
        /// <summary>
        /// Maximum number of loads in the queue
        /// </summary>
        public int Capacity { get; set; }
        /// <summary>
        /// Dequeuing condition for each load
        /// </summary>
        public Func<bool> ToDequeue { get; set; }
        #endregion

        #region Dynamic Properties
        public List<TLoad> Waiting { get; private set; }        
        public int Vancancy { get { return Capacity - Waiting.Count; } }
        
        public HourCounter HourCounter { get; private set; } // statistics
        #endregion

        #region Input Events - Generators
        public Event<TScenario, TStatus> Enqueue(TLoad load) { return new EnqueueEvent(this, load); }
        public Event<TScenario, TStatus> Dequeue() { return new DequeueEvent(this); }
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
            public override string ToString() { return "Enqueue"; }
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
                if (Queue.ToDequeue == null) throw new DequeueConditionNotSpecifiedException();
                if (Queue.ToDequeue())
                {
                    var load = Queue.Waiting.FirstOrDefault();
                    if (load == null) return;
                    load.Log(this);
                    Queue.Waiting.RemoveAt(0);
                    Queue.HourCounter.ObserveChange(-1, ClockTime);
                    foreach (var evnt in Queue.OnDequeue) Execute(evnt(load));
                }
            }
            public override string ToString() { return "Dequeue"; }
        }
        #endregion

        #region Output Events - Reference to Event Generators
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnDequeue { get; set; }
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
        
        // Constructor
        public Queue(int capacity = int.MaxValue, Func<bool> toDequeue = null)
        {
            Capacity = capacity;
            ToDequeue = toDequeue;
            Waiting = new List<TLoad>();  
            HourCounter = new HourCounter(DateTime.MinValue);
            OnDequeue = new List<Func<TLoad, Event<TScenario, TStatus>>>();
        }

        // Warmup method
        public void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }        
    }    
}
