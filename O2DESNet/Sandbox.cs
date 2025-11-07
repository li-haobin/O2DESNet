using Microsoft.Extensions.Logging;

using O2DESNet.HourCounters;
using O2DESNet.Internals;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace O2DESNet;

public abstract class Sandbox<TAssets> : Sandbox where TAssets : IAssets
{

    private readonly TAssets _assets;

    public TAssets Assets => _assets;

    public Sandbox(TAssets assets, string id, int seed) : base(id, seed)
    {
        _assets = assets;
    }

    public Sandbox(ILogger? logger, TAssets assets, string id, int seed) : base(logger, id, seed)
    {
        _assets = assets;
    }
}

public abstract class Sandbox : ISandbox
{
    #region Private Fields
    private readonly string _id;
    private readonly ILogger? _logger;
    private readonly FutureEventList _futureEventList;
    private readonly List<ISandbox> _children = [];
    private readonly List<HourCounter> _hourCounters = [];

    private int _seed;
    private TimeSpan _clockTime = TimeSpan.Zero;
    private Stopwatch? _rtStopwatch = null;
    // Use a private delegate to handle WarmedUp callbacks instead of Action
    private delegate void WarmUpHandler();
    private WarmUpHandler? OnWarmedUp;
    private ISandbox? _parent;
    private Random _defaultRS;
    #endregion

    private FutureEventList FutureEventList => _futureEventList;

    /// <summary>
    /// Tag of the instance of the module
    /// </summary>
    public string Id => _id;

    public ILogger? Logger => _logger;

    protected Random DefaultRS => _defaultRS;

    public int Seed => _seed;

    public Sandbox(string id, int seed)
    {
        _futureEventList = new FutureEventList(this);

        _seed = seed;
        _id = id;
        _defaultRS = new Random(_seed);

        OnWarmedUp += WarmedUpHandler;
    }

    public Sandbox(ILogger? logger, string id, int seed) : this(id, seed)
    {
        _logger = logger;
    }

    private void SetParent(ISandbox parent)
    {
        _parent = parent;
    }

    public void UpdateRandomSeed(int seed)
    {
        _seed = seed;
        _defaultRS = new Random(_seed);
    }

    /// <summary>
    /// Schedule an event to be invoked after the specified time delay
    /// </summary>
    protected void Schedule(Action action, TimeSpan delay, string? tag = null)
    {
        _futureEventList.Add(action, ClockTime + delay, tag);
    }

    /// <summary>
    /// Schedule an event at the current clock time.
    /// </summary>
    protected void Schedule(Action action, string? tag)
    {
        _futureEventList.Add(action, ClockTime, tag);
    }

    #region Simulation Run Control
    internal Event GetHeadEvent()
    {
        var headEvent = _futureEventList.FirstOrDefault();
        foreach (Sandbox child in _children.Cast<Sandbox>())
        {
            var childHeadEvent = child.GetHeadEvent();
            if (headEvent is null || (childHeadEvent is not null &&
                EventComparer.Instance.Compare(childHeadEvent, headEvent) < 0))
                headEvent = childHeadEvent;
        }

        return headEvent;
    }

    public TimeSpan ClockTime
    {
        get
        {
            if (Parent == null)
                return _clockTime;
            return Parent.ClockTime;
        }
    }

    public bool Run()
    {
        if (Parent is not null)
            return Parent.Run();

        var head = GetHeadEvent();

        if (head is null)
            return false;

        head.Owner.FutureEventList.Remove(head);

        _clockTime = head.Timestamp;

        head.Invoke();

        return true;
    }

    public bool Run(TimeSpan duration)
    {
        if (Parent != null)
            return Parent.Run(duration);

        return RunUntil(ClockTime + duration);
    }

    private bool RunUntil(TimeSpan terminate)
    {
        while (true)
        {
            var head = GetHeadEvent();

            if (GetHeadEvent() is not null && GetHeadEvent().Timestamp <= terminate)
            {
                Run();
            }
            else
            {
                _clockTime = terminate;
                return head != null; /// if the simulation can be continued
            }
        }
    }

    public bool Run(int eventCount)
    {
        if (Parent is not null)
            return Parent.Run(eventCount);

        while (eventCount-- > 0)
            if (!Run())
                return false;

        return true;
    }

    public bool Run(double speed)
    {
        if (Parent is not null)
            return Parent.Run(speed);

        var rtn = true;

        if (_rtStopwatch is not null)
        {
            var elapsed = _rtStopwatch.Elapsed.TotalSeconds;
            rtn = RunUntil(ClockTime + TimeSpan.FromSeconds(elapsed * speed));
        }

        _rtStopwatch = Stopwatch.StartNew();

        return rtn;
    }
    #endregion

    #region Children - Sub-modules
    public ISandbox? Parent => _parent;
    public IImmutableList<ISandbox> Children => _children.ToImmutableList();
    public TSandbox AddChild<TSandbox>(TSandbox child) where TSandbox : Sandbox
    {
        _children.Add(child);
        child.SetParent(this);
        OnWarmedUp += child.OnWarmedUp;
        return child;
    }

    public IImmutableList<HourCounter> HourCounters => _hourCounters.ToImmutableList();

    protected HourCounter AddHourCounter(bool keepHistory = false)
    {
        var hc = new HourCounter(Logger, this, keepHistory);
        _hourCounters.Add(hc);
        OnWarmedUp += () => hc.WarmedUp();
        return hc;
    }
    #endregion

    #region Warm-Up
    public bool WarmUp(TimeSpan duration)
    {
        if (Parent != null)
            return Parent.WarmUp(duration);
        var result = RunUntil(ClockTime + duration);
        OnWarmedUp?.Invoke();
        return result; // to be continued
    }
    protected virtual void WarmedUpHandler() { }
    #endregion

    public override string ToString() =>
        string.IsNullOrEmpty(Id) ? GetType().Name : Id;

    public virtual void Dispose()
    {
        foreach (var child in _children)
            child.Dispose();
        foreach (var hc in _hourCounters)
            hc.Dispose();
    }
}
