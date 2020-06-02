using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace O2DESNet.Standard
{
    public class Queue : Sandbox, IQueue
    {
        #region Static Properties
        public double Capacity { get; }
        #endregion

        #region Dynamic Properties        
        public IReadOnlyList<ILoad> PendingToEnqueue => _listPendingToEnqueue.AsReadOnly();
        public IReadOnlyList<ILoad> Queueing => _listQueueing.AsReadOnly();
        public int Occupancy => _listQueueing.Count;
        public double Vacancy => Capacity - Occupancy;
        public double Utilization => AvgNQueueing / Capacity;
        public double AvgNQueueing => HCQueueing.AverageCount;

        private readonly List<ILoad> _listQueueing = new List<ILoad>();
        private readonly List<ILoad> _listPendingToEnqueue = new List<ILoad>();
        private HourCounter HCQueueing { get; }
        #endregion

        #region  Methods / Events
        public void RequestEnqueue(ILoad load)
        {
            Log("RequestEnqueue");
            if (DebugMode) Debug.WriteLine("{0}:\t{1}\tRequestEnqueue\t{2}", ClockTime, this, load);
            _listPendingToEnqueue.Add(load);
            AttemptEnqueue();
        }

        public void Dequeue(ILoad load)
        {
            if (_listQueueing.Contains(load))
            {
                Log("Dequeue", load);
                if (DebugMode)
                    Debug.WriteLine("{0}:\t{1}\tDequeue\t{2}", ClockTime, this, load);
                _listQueueing.Remove(load);
                HCQueueing.ObserveChange(-1, ClockTime);
                AttemptEnqueue();
            }
        }

        private void AttemptEnqueue()
        {
            if (_listPendingToEnqueue.Count > 0 && _listQueueing.Count < Capacity)
            {                
                var load = _listPendingToEnqueue.First();
                Log("Enqueue", load);
                if (DebugMode) Debug.WriteLine("{0}:\t{1}\tEnqueue\t{2}", ClockTime, this, load);
                _listQueueing.Add(load);
                _listPendingToEnqueue.RemoveAt(0);
                HCQueueing.ObserveChange(1, ClockTime);
                OnEnqueued.Invoke(load);
            }
        }

        public event Action<ILoad> OnEnqueued = load => { };
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Queue"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="seed">The seed.</param>
        /// <param name="id">The identifier.</param>
        public Queue(double capacity, int seed, string id) 
            : base(seed, id, Pointer.Empty)
        {
            Capacity = capacity;
            HCQueueing = AddHourCounter();
        }

        public override void Dispose()
        {
            foreach (var @delegate in OnEnqueued.GetInvocationList())
            {
                if (@delegate == null) continue;
                var i = @delegate as Action<ILoad>;
                OnEnqueued -= i;
            }
        }        
    }
}
