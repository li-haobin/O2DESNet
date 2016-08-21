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
        where TLoad : Load
    {
        public List<TLoad> Waiting { get; private set; }
        public int Capacity { get; private set; }
        public int Vancancy { get { return Capacity - Waiting.Count; } }
        public bool HasVacancy() { return Vancancy > 0; }
        /// <summary>
        /// Dequeuing condition for each load
        /// </summary>
        public Func<bool> ToDequeue { get; set; }
        /// <summary>
        /// Action to execute for each load
        /// </summary>
        public Action<TLoad> OnDequeue { get; set; }
        public HourCounter HourCounter { get; private set; }

        public Queue(int capacity = int.MaxValue)
        {
            Waiting = new List<TLoad>();
            Capacity = capacity;
            HourCounter = new HourCounter(DateTime.MinValue);
        }

        public void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }

        /// <summary>
        /// Attempt to dequeue the first load
        /// </summary>
        /// <returns>If the first is dequeued</returns>
        public bool AttemptDequeue(DateTime clockTime)
        {
            if (Waiting.Count == 0 || !ToDequeue()) return false;
            OnDequeue(Waiting.FirstOrDefault());
            Waiting.RemoveAt(0);
            HourCounter.ObserveChange(-1, clockTime);
            return true;
        }

        public Enqueue EnqueueEvent(TLoad load)
        {
            return new Enqueue(this, load);
        }

        public class Enqueue : Event<TScenario, TStatus>
        {
            public Queue<TScenario, TStatus, TLoad> Queue { get; private set; }
            public TLoad Load { get; private set; }

            internal Enqueue(Queue<TScenario, TStatus, TLoad> queue, TLoad load)
            {
                Queue = queue;
                Load = load;
            }
            public override void Invoke()
            {
                //enqueue if has vacancy
                if (!Queue.HasVacancy()) throw new Exception("The Queue does not have vacancy.");
                Queue.Waiting.Add(Load);
                Queue.HourCounter.ObserveChange(1, ClockTime);

                Queue.AttemptDequeue(ClockTime);
            }
        }
    }    
}
