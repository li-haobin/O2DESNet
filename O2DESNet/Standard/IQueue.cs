using System;
using System.Collections.Generic;

namespace O2DESNet.Standard
{
    public interface IQueue : ISandbox 
    {
        double Capacity { get; }

        IReadOnlyList<ILoad> PendingToEnqueue { get; }
        IReadOnlyList<ILoad> Queueing { get; }        
        int Occupancy { get; }
        double Vacancy { get; }
        double Utilization { get; }
        /// <summary>
        /// Average number of loads queueing
        /// </summary>
        double AvgNQueueing { get; }

        void RqstEnqueue(ILoad load);
        void Dequeue(ILoad load);

        event Action<ILoad> OnEnqueued;
    }
}
