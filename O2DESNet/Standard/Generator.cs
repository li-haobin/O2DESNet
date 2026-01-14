using Microsoft.Extensions.Logging;

using System;
using System.Linq;

namespace O2DESNet.Standard;

/// <summary>
/// A generic event generator that produces Arrive events according to a provided inter-arrival time sampler.
/// 
/// Big picture
/// - Users supply a function InterArrivalTime(Random) that yields the time gap until the next arrival.
/// - Start() enables generation and schedules the next arrival; End() disables further generation.
/// - Each Arrive event increments Count and immediately schedules the next arrival using the sampler.
/// - WarmUp() resets Count via the sandbox warm-up mechanism, keeping statistics separate if desired.
/// </summary>
public class Generator : Sandbox<Generator.Statics>, IGenerator
{
    /// <summary>
    /// Static configuration for the generator: an RNG-based inter-arrival time sampler.
    /// </summary>
    public class Statics : IAssets
    {
        /// <summary>
        /// Identifier of the configuration. Defaults to the type name.
        /// </summary>
        public string Id => GetType().Name;
        /// <summary>
        /// Function to sample the inter-arrival time using the provided RNG. Must be set before Start().
        /// </summary>
        public Func<Random, TimeSpan>? InterArrivalTime { get; set; }
        public Generator Sandbox(ILogger? logger, int seed) => new(logger, this, nameof(Generator), seed);
    }

    #region Dyanmic Properties
    /// <summary>
    /// The clock time when Start() was called, null before the generator is started.
    /// </summary>
    public TimeSpan? StartTime { get; private set; }

    /// <summary>
    /// Indicates whether the generator is actively producing arrivals.
    /// </summary>
    public bool IsOn { get; private set; }
    /// <summary>
    /// Total number of arrivals generated since last warm-up/reset.
    /// </summary>
    public int Count { get; private set; } // number of loads generated   
    #endregion

    #region Events
    /// <summary>
    /// Start generating arrivals. Schedules the first arrival using the inter-arrival sampler.
    /// </summary>
    public void Start()
    {
        if (!IsOn)
        {
            Logger?.LogInformation("Start");
            Logger?.LogDebug($"{ClockTime}:\t{this}\tStart");
            if (Assets.InterArrivalTime == null)
                throw new Exception("Inter-arrival time is null");
            IsOn = true;
            StartTime = ClockTime;
            Count = 0;
            ScheduleToArrive();
        }
    }

    /// <summary>
    /// Stop generating further arrivals. Pending scheduled candidates will be ignored when fired (due to IsOn check).
    /// </summary>
    public void End()
    {
        if (IsOn)
        {
            Logger?.LogInformation("End");
            Logger?.LogDebug($"{ClockTime}:\t{this}\tEnd");
            IsOn = false;
        }
    }

    /// <summary>
    /// Schedule the next arrival according to the provided inter-arrival sampler.
    /// </summary>
    private void ScheduleToArrive()
    {
        Schedule(Arrive, Assets.InterArrivalTime(DefaultRS));
    }

    /// <summary>
    /// Arrival event handler. Increments Count, schedules the next arrival, and emits OnArrive.
    /// </summary>
    private void Arrive()
    {
        if (IsOn)
        {
            Logger?.LogInformation("Arrive");
            Logger?.LogDebug($"{ClockTime}:\t{this}\tArrive");

            Count++;
            ScheduleToArrive();
            OnArrive.Invoke();
        }
    }

    /// <summary>
    /// Event raised whenever an arrival is realized.
    /// </summary>
    public event Action OnArrive = () => { };
    #endregion

    /// <summary>
    /// Convenience constructor without logger.
    /// </summary>
    public Generator(Statics assets, string id, int seed)
        : this(null, assets, id, seed) { }

    /// <summary>
    /// Initializes dynamic state.
    /// </summary>
    public Generator(ILogger? logger, Statics assets, string id, int seed)
        : base(logger, assets, id, seed)
    {
        IsOn = false;
        Count = 0;
    }

    /// <summary>
    /// Reset dynamic counters after warm-up.
    /// </summary>
    protected override void WarmedUpHandler()
    {
        Count = 0;
    }

    /// <summary>
    /// Unsubscribe listeners to avoid leaks and dispose base resources.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();
        var handlers = OnArrive.GetInvocationList().Cast<Action>();
        foreach (Action i in handlers)
        {
            OnArrive -= i;
        }
    }
}
