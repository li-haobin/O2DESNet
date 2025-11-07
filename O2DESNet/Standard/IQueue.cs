using System;
using System.Collections.Generic;

namespace O2DESNet.Standard;

public interface IQueue : ISandbox
{
    double Capacity { get; }

    IReadOnlyList<IEntity> PendingToEnqueue { get; }
    IReadOnlyList<IEntity> Queueing { get; }
    int Occupancy { get; }
    double Vacancy { get; }
    double Utilization { get; }
    /// <summary>
    /// Average number of loads queueing
    /// </summary>
    double AvgNQueueing { get; }

    void RqstEnqueue(IEntity load);
    void Dequeue(IEntity load);

    event Action<IEntity> OnEnqueued;
}
