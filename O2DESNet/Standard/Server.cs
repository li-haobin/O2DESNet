using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace O2DESNet.Standard
{
    public class Server : Sandbox<Server.Statics>, IServer
    {
        public class Statics : IAssets
        {
            public string Id => GetType().Name;
            public double Capacity { get; set; }
            public Func<Random, ILoad, TimeSpan> ServiceTime { get; set; }
        }

        #region Dynamic Properties
        public double Capacity => Assets.Capacity;
        public int Occupancy => _hSetServing.Count + _hSetPendingToDepart.Count;
        public double Vacancy => Capacity - Occupancy;
        public double AvgNServing => HCServing.AverageCount;
        public double AvgNOccupying => HCServing.AverageCount + HCPendingToDepart.AverageCount;
        public double UtilServing => AvgNServing / Capacity;
        public double UtilOccupying => AvgNOccupying / Capacity;
        public IReadOnlyList<ILoad> PendingToStart => _listPendingToStart.AsReadOnly();
        public IReadOnlyList<ILoad> Serving => _hSetServing.ToList().AsReadOnly();
        public IReadOnlyList<ILoad> PendingToDepart => _hSetPendingToDepart.ToList().AsReadOnly();

        private HourCounter HCServing { get; }
        private HourCounter HCPendingToDepart { get; }
        private readonly List<ILoad> _listPendingToStart = new List<ILoad>();
        private readonly HashSet<ILoad> _hSetServing = new HashSet<ILoad>();
        private readonly HashSet<ILoad> _hSetPendingToDepart = new HashSet<ILoad>();
        #endregion

        #region Events
        public void RequestStart(ILoad load)
        {
            Log("Request to Start", load);
            if (DebugMode) Debug.WriteLine("{0}:\t{1}\tRequestStart\t{2}", ClockTime, this, load);
            _listPendingToStart.Add(load);
            AttemptStart();
        }

        private void AttemptStart()
        {
            if (_listPendingToStart.Count > 0 && Vacancy > 0)
            {
                var load = _listPendingToStart.First();
                Log("Start", load);
                if (DebugMode) Debug.WriteLine("{0}:\t{1}\tStart\t{2}", ClockTime, this, load);
                _listPendingToStart.RemoveAt(0);
                _hSetServing.Add(load);
                HCServing.ObserveChange(1, ClockTime);
                OnStarted.Invoke(load);
                Schedule(() => ReadyToDepart(load), Assets.ServiceTime(DefaultRS, load));
            }
        }

        private void ReadyToDepart(ILoad load)
        {
            Log("Ready to Depart", load);
            if (DebugMode) Debug.WriteLine("{0}:\t{1}\tReadyToDepart\t{2}", ClockTime, this, load);
            _hSetServing.Remove(load);
            _hSetPendingToDepart.Add(load);
            HCServing.ObserveChange(-1, ClockTime);
            HCPendingToDepart.ObserveChange(1, ClockTime);
            OnReadyToDepart.Invoke(load);
        }

        public void Depart(ILoad load)
        {
            if (_hSetPendingToDepart.Contains(load))
            {
                Log("Depart", load);
                if (DebugMode) Debug.WriteLine("{0}:\t{1}\tDepart\t{2}", ClockTime, this, load);
                _hSetPendingToDepart.Remove(load);
                HCPendingToDepart.ObserveChange(-1, ClockTime);
                AttemptStart();
            }
        }

        public event Action<ILoad> OnStarted = load => { };
        public event Action<ILoad> OnReadyToDepart = load => { };
        #endregion

        public Server(Statics assets, int seed = 0, string id = null)
            : base(assets, seed, id)
        {
            HCServing = AddHourCounter();
            HCPendingToDepart = AddHourCounter();
        }

        public override void Dispose()
        {
            foreach (Action<ILoad> i in OnStarted.GetInvocationList()) OnStarted -= i;
            foreach (Action<ILoad> i in OnReadyToDepart.GetInvocationList()) OnReadyToDepart -= i;
        }
    }
}
