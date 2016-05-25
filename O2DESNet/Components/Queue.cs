using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Queue<TLoad> where TLoad : Load
    {
        public List<TLoad> Waiting { get; private set; }
        public int Capacity { get; private set; }
        public int Vancancy { get { return Capacity - Waiting.Count; } }
        public bool HasVacancy { get { return Vancancy > 0; } }
        /// <summary>
        /// Dequeuing condition for each load
        /// </summary>
        public Dictionary<TLoad, Func<bool>> ToDequeues { get; private set; }
        /// <summary>
        /// Action to execute for each load
        /// </summary>
        public Dictionary<TLoad, Action<TLoad>> OnDequeues { get; private set; }
        public HourCounter HourCounter { get; private set; }

        public Queue(int capacity = int.MaxValue)
        {
            Waiting = new List<TLoad>();
            Capacity = capacity;
            ToDequeues = new Dictionary<TLoad, Func<bool>>();
            OnDequeues = new Dictionary<TLoad, Action<TLoad>>();
            HourCounter = new HourCounter(DateTime.MinValue);
        }

        public void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }

        public void Enqueue(TLoad load, Func<bool> toDequeue, Action<TLoad> onDequeue, DateTime clockTime)
        {
            if (!HasVacancy) throw new Exception("The Queue does not have vacancy.");
            Waiting.Add(load);
            ToDequeues.Add(load, toDequeue);
            OnDequeues.Add(load, onDequeue);
            HourCounter.ObserveChange(1, clockTime);
        }

        /// <summary>
        /// Attempt to dequeue the first load
        /// </summary>
        /// <returns>If the first is dequeued</returns>
        public bool AttemptDequeue(DateTime clockTime) {
            var load = Waiting.FirstOrDefault();
            if (load == null || !ToDequeues[load]()) return false;
            OnDequeues[load](load);
            Waiting.RemoveAt(0);
            ToDequeues.Remove(load);
            OnDequeues.Remove(load);
            HourCounter.ObserveChange(-1, clockTime);
            return true;
        }

        /// <summary>
        /// Attempt to dequeue all loads in the queue in sequence
        /// </summary> 
        /// <returns>Number of loads dequeued</returns>
        //public int AttemptDequeueAll() {
        //    int index = 0;
        //    int dequeued = 0;
        //    while(index < Count)
        //    {
        //        var load = this.ElementAt(index);
        //        if (ToDequeues[load]())
        //        {
        //            OnDequeues[load](load);
        //            RemoveAt(index);
        //            ToDequeues.Remove(load);
        //            OnDequeues.Remove(load);
        //            dequeued++;
        //        }
        //        else index++;
        //    }
        //    return dequeued;
        //}
    }
}
