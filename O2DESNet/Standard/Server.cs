using Microsoft.Extensions.Logging;

using O2DESNet.HourCounters;

using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Standard;

/// <summary>
/// A capacity-constrained service facility that starts, serves, and completes loads.
/// 
/// Big picture
/// - Clients call RqstStart(load) to request service. The server admits the load if capacity is available,
///   otherwise the request waits in a pending list.
/// - When a load starts service, the server samples a service time and schedules a completion (ReadyToDepart).
/// - Upon completion, the load moves to a pending-to-depart set until Depart(load) is called by an external coordinator.
/// - The server tracks time-weighted statistics with HourCounter instances for Serving and PendingToDepart states.
/// </summary>
public class Server : Sandbox<Server.Statics>, IServer
{
    /// <summary>
    /// Static configuration for the server: capacity and service-time distribution.
    /// </summary>
    public class Statics : IAssets
    {
        /// <summary>
        /// Identifier of the configuration. Defaults to the type name.
        /// </summary>
        public string Id => GetType().Name;
        /// <summary>
        /// Maximum number of loads that can be concurrently in service or pending to depart.
        /// </summary>
        public double Capacity { get; set; }
        /// <summary>
        /// Function to sample a service time given a RNG and the load. Must be set by the user.
        /// </summary>
        public Func<Random, IEntity, TimeSpan>? ServiceTime { get; set; }
    }

    #region Dynamic Properties
    /// <summary>
    /// Configured capacity from assets.
    /// </summary>
    public double Capacity => Assets.Capacity;
    /// <summary>
    /// Total number of loads occupying the server (serving + pending to depart).
    /// </summary>
    public int Occupancy => HSet_Serving.Count + HSet_PendingToDepart.Count;
    /// <summary>
    /// Remaining capacity available for starting new loads.
    /// </summary>
    public double Vacancy => Capacity - Occupancy;
    /// <summary>
    /// Time-average number of loads being served (active) over the simulation horizon.
    /// </summary>
    public double AvgNServing => HC_Serving.AverageCount;
    /// <summary>
    /// Time-average number of loads occupying the server (active + passive) over the simulation horizon.
    /// </summary>
    public double AvgNOccupying => HC_Serving.AverageCount + HC_PendingToDepart.AverageCount;
    /// <summary>
    /// Utilization considering only actively served loads.
    /// </summary>
    public double UtilServing => AvgNServing / Capacity;
    /// <summary>
    /// Utilization including both serving and pending-to-depart loads.
    /// </summary>
    public double UtilOccupying => AvgNOccupying / Capacity;
    /// <summary>
    /// Loads waiting to start (FIFO by list order).
    /// </summary>
    public IReadOnlyList<IEntity> PendingToStart => List_PendingToStart.AsReadOnly();
    /// <summary>
    /// Loads currently in service.
    /// </summary>
    public IReadOnlyList<IEntity> Serving => HSet_Serving.ToList().AsReadOnly();
    /// <summary>
    /// Loads that completed service and are awaiting explicit departure.
    /// </summary>
    public IReadOnlyList<IEntity> PendingToDepart => HSet_PendingToDepart.ToList().AsReadOnly();

    // Time-weighted counters for statistics
    private HourCounter HC_Serving { get; set; }
    private HourCounter HC_PendingToDepart { get; set; }

    // Internal state containers
    private readonly List<IEntity> List_PendingToStart = [];
    private readonly HashSet<IEntity> HSet_Serving = [];
    private readonly HashSet<IEntity> HSet_PendingToDepart = [];
    #endregion

    #region Events
    /// <summary>
    /// Request to start serving a load. The load is queued to PendingToStart and an attempt is made immediately.
    /// </summary>
    public void RqstStart(IEntity load)
    {
        Logger?.LogInformation("Request to Start", load);
        Logger?.LogDebug($"{ClockTime}:\t{this}\tRqstStart\t{load}");
        List_PendingToStart.Add(load);
        AtmptStart();
    }

    /// <summary>
    /// Try to start service if there is a pending load and available capacity.
    /// On start: the load moves from pending to serving, serving counter +1, and a completion is scheduled.
    /// </summary>
    private void AtmptStart()
    {
        if (List_PendingToStart.Count > 0 && Vacancy > 0)
        {
            var load = List_PendingToStart.First();
            Logger?.LogInformation("Start", load);
            Logger?.LogDebug($"{ClockTime}:\t{this}\tStart\t{load}");
            List_PendingToStart.RemoveAt(0);
            HSet_Serving.Add(load);
            HC_Serving.ObserveChange(1, ClockTime);
            OnStarted.Invoke(load);
            // Schedule completion of service using the provided ServiceTime sampler
            Schedule(() => ReadyToDepart(load), Assets.ServiceTime(DefaultRS, load));
        }
    }

    /// <summary>
    /// Service completion handler. Moves the load from Serving to PendingToDepart and updates counters.
    /// </summary>
    private void ReadyToDepart(IEntity load)
    {
        Logger?.LogInformation("Ready to Depart", load);
        Logger?.LogDebug($"{ClockTime}:\t{this}\tReadyToDepart\t{load}");
        HSet_Serving.Remove(load);
        HSet_PendingToDepart.Add(load);
        HC_Serving.ObserveChange(-1, ClockTime);
        HC_PendingToDepart.ObserveChange(1, ClockTime);
        OnReadyToDepart.Invoke(load);
    }

    /// <summary>
    /// Explicitly depart a completed load (if present). Frees occupancy and triggers a new start attempt.
    /// </summary>
    public void Depart(IEntity load)
    {
        if (HSet_PendingToDepart.Contains(load))
        {
            Logger?.LogInformation("Depart", load);
            Logger?.LogDebug($"{ClockTime}:\t{this}\tDepart\t{load}");
            HSet_PendingToDepart.Remove(load);
            HC_PendingToDepart.ObserveChange(-1, ClockTime);
            // New capacity may be available; try to start the next waiting load
            AtmptStart();
        }
    }

    /// <summary>
    /// Event raised when a load has just started service.
    /// </summary>
    public event Action<IEntity> OnStarted = Load => { };
    /// <summary>
    /// Event raised when a load has completed service and is ready to depart.
    /// </summary>
    public event Action<IEntity> OnReadyToDepart = load => { };
    #endregion

    /// <summary>
    /// Convenience constructor without logger.
    /// </summary>
    public Server(Statics assets, string id, int seed)
        : this(null, assets, id, seed) { }

    /// <summary>
    /// Initializes the server and its time-weighted counters.
    /// </summary>
    public Server(ILogger? logger, Statics assets, string id, int seed)
        : base(logger, assets, id, seed)
    {
        HC_Serving = AddHourCounter();
        HC_PendingToDepart = AddHourCounter();
    }

    /// <summary>
    /// Unsubscribe listeners to avoid leaks.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();
        var OnStartedHandlers = OnStarted.GetInvocationList().Cast<Action<IEntity>>();
        foreach (Action<IEntity> i in OnStartedHandlers)
        {
            OnStarted -= i;
        }

        var OnReadyToDepartHandlers = OnReadyToDepart.GetInvocationList().Cast<Action<IEntity>>();
        foreach (Action<IEntity> i in OnReadyToDepartHandlers)
        {
            OnReadyToDepart -= i;
        }
    }
}
