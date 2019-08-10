using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace O2DESNet.Standard
{
    public class Queue : Sandbox, IQueue
    {
        #region Static Properties
        public double Capacity { get; private set; }
        #endregion

        #region Dynamic Properties        
        public IReadOnlyList<ILoad> PendingToEnqueue { get { return List_PendingToEnqueue.AsReadOnly(); } }
        public IReadOnlyList<ILoad> Queueing { get { return List_Queueing.AsReadOnly(); } }
        public int Occupancy { get { return List_Queueing.Count; } }
        public double Vacancy { get { return Capacity - Occupancy; } }
        public double Utilization { get { return AvgNQueueing / Capacity; } }
        public double AvgNQueueing { get{ return HC_Queueing.AverageCount; } }

        private readonly List<ILoad> List_Queueing = new List<ILoad>();
        private readonly List<ILoad> List_PendingToEnqueue = new List<ILoad>();
        private HourCounter HC_Queueing { get; set; }
        #endregion

        #region  Methods / Events
        public void RqstEnqueue(ILoad load)
        {
            Log("RqstEnqueue");
            if (DebugMode) Debug.WriteLine("{0}:\t{1}\tRqstEnqueue\t{2}", ClockTime, this, load);
            List_PendingToEnqueue.Add(load);
            AtmptEnqueue();
        }
        public void Dequeue(ILoad load)
        {
            if (List_Queueing.Contains(load))
            {
                Log("Dequeue", load);
                if (DebugMode) Debug.WriteLine("{0}:\t{1}\tDequeue\t{2}", ClockTime, this, load);
                List_Queueing.Remove(load);
                HC_Queueing.ObserveChange(-1, ClockTime);
                AtmptEnqueue();
            }
        }
        private void AtmptEnqueue()
        {
            if (List_PendingToEnqueue.Count > 0 && List_Queueing.Count < Capacity)
            {                
                var load = List_PendingToEnqueue.First();
                Log("Enqueue", load);
                if (DebugMode) Debug.WriteLine("{0}:\t{1}\tEnqueue\t{2}", ClockTime, this, load);
                List_Queueing.Add(load);
                List_PendingToEnqueue.RemoveAt(0);
                HC_Queueing.ObserveChange(1, ClockTime);
                OnEnqueued.Invoke(load);
            }
        }

        public event Action<ILoad> OnEnqueued = load => { };
        #endregion

        public Queue(double capacity, int seed = 0, string id = null) 
            : base(seed, id)
        {
            Capacity = capacity;
            HC_Queueing = AddHourCounter();
        }

        public override void Dispose()
        {
            foreach (Action<ILoad> i in OnEnqueued.GetInvocationList()) OnEnqueued -= i;
        }        
    }
}
