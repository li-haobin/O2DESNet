using Microsoft.Extensions.Logging;

using O2DESNet.HourCounters;

using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Standard;

/// <summary>
/// A bounded FIFO buffer that accepts loads, tracks its occupancy over time, and emits an event
/// when a load is successfully enqueued.
/// 
/// Big picture
/// - RqstEnqueue(load) submits a load to the queue. If there is space, it is immediately enqueued;
///   otherwise it waits in a pending list.
/// - When space becomes available (via Dequeue or a prior enqueue), AtmptEnqueue tries to admit the first pending load.
/// - HourCounter HC_Queueing captures time-weighted occupancy statistics (average queue length, utilization, etc.).
/// </summary>
public class Queue : Sandbox, IQueue
{
    #region Static Properties
    /// <summary>
    /// Maximum number of loads that the queue can hold concurrently.
    /// </summary>
    public double Capacity { get; private set; }
    #endregion

    #region Dynamic Properties        
    /// <summary>
    /// Loads waiting to be enqueued because the queue was full at request time.
    /// </summary>
    public IReadOnlyList<IEntity> PendingToEnqueue => List_PendingToEnqueue.AsReadOnly();
    /// <summary>
    /// Loads currently occupying the queue (FIFO by insertion order).
    /// </summary>
    public IReadOnlyList<IEntity> Queueing => List_Queueing.AsReadOnly();
    /// <summary>
    /// Current number of loads in the queue.
    /// </summary>
    public int Occupancy => List_Queueing.Count;
    /// <summary>
    /// Remaining capacity available for new loads.
    /// </summary>
    public double Vacancy => Capacity - Occupancy;
    /// <summary>
    /// Time-average fraction of capacity occupied, derived from the hour counter.
    /// </summary>
    public double Utilization => AvgNQueueing / Capacity;
    /// <summary>
    /// Time-average number of loads in the queue.
    /// </summary>
    public double AvgNQueueing => HC_Queueing.AverageCount;

    private readonly List<IEntity> List_Queueing = [];
    private readonly List<IEntity> List_PendingToEnqueue = [];
    private HourCounter HC_Queueing { get; set; }
    #endregion

    #region  Methods / Events
    /// <summary>
    /// Request to enqueue a load. If capacity allows, the load is enqueued immediately; otherwise
    /// the load is stored in PendingToEnqueue until space is available.
    /// </summary>
    public void RqstEnqueue(IEntity load)
    {
        Logger?.LogInformation("RqstEnqueue");
        Logger?.LogDebug($"{ClockTime}:\t{this}\tRqstEnqueue\t{load}");
        List_PendingToEnqueue.Add(load);
        AtmptEnqueue();
    }

    /// <summary>
    /// Remove a specific load from the queue if present, and attempt to admit the next pending load.
    /// </summary>
    public void Dequeue(IEntity load)
    {
        if (List_Queueing.Contains(load))
        {
            Logger?.LogInformation("Dequeue", load);
            Logger?.LogDebug($"{ClockTime}:\t{this}\tDequeue\t{load}");
            List_Queueing.Remove(load);
            HC_Queueing.ObserveChange(-1, ClockTime);
            AtmptEnqueue();
        }
    }
    /// <summary>
    /// Try to enqueue the first pending load if capacity is available. On success, updates
    /// the time-weighted occupancy and emits OnEnqueued(load).
    /// </summary>
    private void AtmptEnqueue()
    {
        if (List_PendingToEnqueue.Count > 0 && List_Queueing.Count < Capacity)
        {
            var load = List_PendingToEnqueue.First();
            Logger?.LogInformation("Enqueue", load);
            Logger?.LogDebug($"{ClockTime}:\t{this}\tEnqueue\t{load}");
            List_Queueing.Add(load);
            List_PendingToEnqueue.RemoveAt(0);
            HC_Queueing.ObserveChange(1, ClockTime);
            OnEnqueued.Invoke(load);
        }
    }

    /// <summary>
    /// Event raised when a load has been successfully enqueued.
    /// </summary>
    public event Action<IEntity> OnEnqueued = load => { };
    #endregion

    /// <summary>
    /// Convenience constructor without logger.
    /// </summary>
    public Queue(double capacity, string id, int seed)
        : this(null, capacity, id, seed) { }

    /// <summary>
    /// Initializes the queue with a fixed capacity and a time-weighted occupancy counter.
    /// </summary>
    public Queue(ILogger? logger, double capacity, string id, int seed)
        : base(logger, id, seed)
    {
        Capacity = capacity;
        HC_Queueing = AddHourCounter();
    }

    /// <summary>
    /// Unsubscribe listeners to avoid leaks and dispose base resources.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();
        var handlers = OnEnqueued.GetInvocationList().Cast<Action<IEntity>>();
        foreach (Action<IEntity> i in handlers)
        {
            OnEnqueued -= i;
        }
    }
}
