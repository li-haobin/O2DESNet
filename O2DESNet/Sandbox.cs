using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace O2DESNet
{
    public abstract class Sandbox<TAssets> : Sandbox
        where TAssets : IAssets
    {
        public TAssets Assets { get; }

        protected Sandbox(TAssets assets, int seed = 0, string id = null, Pointer pointer = new Pointer())
            : base(seed, id, pointer) { Assets = assets; }
    }

    public abstract class Sandbox : ISandbox
    {
        #region Private Fields
        private static int _count;

        private readonly int _index;
        private readonly string _id;
        private readonly Pointer _pointer;

        private DateTime _clockTime = DateTime.MinValue;
        private DateTime? _realTimeForLastRun;

        private int _seed;
        private bool _isDisposed;
        private Action _onWarmedUp;
        private string _logFile;
        private Random _defaultRS;
        #endregion

        /// <summary>
        /// Unique index in sequence for all module instances 
        /// </summary>
        public int Index => _index;

        /// <summary>
        /// Tag of the instance of the module
        /// </summary>
        public string Id => _id;

        public Pointer Pointer => _pointer;

        protected Random DefaultRS => _defaultRS;

        public int Seed
        {
            get => _seed;
            set
            {
                _seed = value;
                new Action(SetRandomSeed).Invoke();
            }
        }

        private void SetRandomSeed()
        {
            _defaultRS = new Random(_seed);
        }

        #region Future Event List
        internal SortedSet<Event> FutureEventList = new SortedSet<Event>(EventComparer.Instance);
        /// <summary>
        /// Schedule an event to be invoked at the specified clock-time
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="clockTime">The clock time.</param>
        /// <param name="tag">The tag.</param>
        protected void Schedule(Action action, DateTime clockTime, string tag = null)
        {
            FutureEventList.Add(new Event(this, action, clockTime, tag));
        }

        /// <summary>
        /// Schedule an event to be invoked after the specified time delay
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="tag">The tag.</param>
        protected void Schedule(Action action, TimeSpan delay, string tag = null)
        {
            FutureEventList.Add(new Event(this, action, ClockTime + delay, tag));
        }

        /// <summary>
        /// Schedule an event at the current clock time.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="tag">The tag.</param>
        protected void Schedule(Action action, string tag = null)
        {
            FutureEventList.Add(new Event(this, action, ClockTime, tag));
        }
        #endregion

        #region Simulation Run Control
        internal Event HeadEvent
        {
            get
            {
                var headEvent = FutureEventList.FirstOrDefault();
                foreach (Sandbox child in _childrenList)
                {
                    var childHeadEvent = child.HeadEvent;
                    if (headEvent == null || (childHeadEvent != null &&
                        EventComparer.Instance.Compare(childHeadEvent, headEvent) < 0))
                        headEvent = childHeadEvent;
                }
                return headEvent;
            }
        }

        public DateTime ClockTime
        {
            get
            {
                if (Parent == null) return _clockTime;
                return Parent.ClockTime;
            }
        }
        public DateTime? HeadEventTime
        {
            get
            {
                var head = HeadEvent;
                if (head == null) return null;
                return head.ScheduledTime;
            }
        }
        public bool Run()
        {
            if (Parent != null) return Parent.Run();
            var head = HeadEvent;
            if (head == null) return false;
            head.Owner.FutureEventList.Remove(head);
            _clockTime = head.ScheduledTime;
            head.Invoke();
            return true;
        }

        public bool Run(TimeSpan duration)
        {
            if (Parent != null) return Parent.Run(duration);
            return Run(ClockTime.Add(duration));
        }

        public bool Run(DateTime terminate)
        {
            if (Parent != null) return Parent.Run(terminate);
            while (true)
            {
                var head = HeadEvent;
                if (HeadEvent != null && HeadEvent.ScheduledTime <= terminate) Run();
                else
                {
                    _clockTime = terminate;
                    return head != null; /// if the simulation can be continued
                }
            }
        }

        public bool Run(int eventCount)
        {
            if (Parent != null) return Parent.Run(eventCount);
            while (eventCount-- > 0)
                if (!Run()) return false;
            return true;
        }

        public bool Run(double speed)
        {
            if (Parent != null) return Parent.Run(speed);
            var rtn = true;
            if (_realTimeForLastRun != null)
                rtn = Run(terminate: ClockTime.AddSeconds((DateTime.Now - _realTimeForLastRun.Value).TotalSeconds * speed));
            _realTimeForLastRun = DateTime.Now;
            return rtn;
        }
        #endregion

        #region Children - Sub-modules
        public ISandbox Parent { get; private set; }

        private readonly List<ISandbox> _childrenList = new List<ISandbox>();

        public IReadOnlyList<ISandbox> Children => _childrenList.AsReadOnly();

        protected TSandbox AddChild<TSandbox>(TSandbox child) where TSandbox : Sandbox
        {
            _childrenList.Add(child);
            child.Parent = this;
            _onWarmedUp += child._onWarmedUp;
            return child;
        }

        /// <summary>
        /// Gets the hour counters.
        /// </summary>
        protected IReadOnlyList<HourCounter> HourCounters => _hourCountersList.AsReadOnly();

        private readonly List<HourCounter> _hourCountersList = new List<HourCounter>();

        /// <summary>
        /// Adds the hour counter.
        /// </summary>
        /// <param name="keepHistory">if set to <c>true</c> [keep history].</param>
        protected HourCounter AddHourCounter(bool keepHistory = false)
        {
            var hc = new HourCounter(this, keepHistory);
            _hourCountersList.Add(hc);
            _onWarmedUp += () => hc.WarmedUp();
            return hc;
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Sandbox"/> class.
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="pointer">The pointer.</param>
        protected Sandbox(int seed, string id, Pointer pointer)
        {
            _id = id;
            _index = _count++;
            _pointer = pointer;
            _onWarmedUp += WarmedUpHandler;

            _seed = seed;
            new Action(SetRandomSeed).Invoke();
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var str = Id;
            if (string.IsNullOrEmpty(str)) str = GetType().Name;
            str += "#" + Index.ToString();
            return str;
        }

        #region Warm-Up
        /// <summary>
        /// Warms up within specified period.
        /// </summary>
        /// <param name="period">The period.</param>
        public bool WarmUp(TimeSpan period)
        {
            return Parent?.WarmUp(period) ?? WarmUp(ClockTime + period);
        }

        /// <summary>
        /// Warms up till specified time.
        /// </summary>
        /// <param name="till">The till.</param>
        public bool WarmUp(DateTime till)
        {
            if (Parent != null) return Parent.WarmUp(till);
            var result = Run(till);
            _onWarmedUp.Invoke();
            return result; // to be continued
        }

        /// <summary>
        /// Warmed up handler.
        /// </summary>
        protected virtual void WarmedUpHandler() { }
        #endregion

        #region For Logging
        public string LogFile
        {
            get => _logFile;
            set
            {
                _logFile = value; if (_logFile != null) using (var sw = new StreamWriter(_logFile)) { };
            }
        }
        protected void Log(params object[] args)
        {
            var timeStr = ClockTime.ToString("y/M/d H:mm:ss.fff");
            if (LogFile != null)
            {
                using (var sw = new StreamWriter(LogFile, true))
                {
                    sw.Write("{0}\t{1}\t", timeStr, Id);
                    foreach (var arg in args) sw.Write("{0}\t", arg);
                    sw.WriteLine();
                }
            }
        }

        public bool DebugMode { get; set; } = false;
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_isDisposed)
                {
                    if (_childrenList != null)
                        foreach (var child in _childrenList) child?.Dispose();

                    if (_hourCountersList != null)
                        foreach (var hc in _hourCountersList) hc?.Dispose();
                }

                _isDisposed = true;
            }
        }
        #endregion
    }
}
