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
            public string Id { get { return GetType().Name; } }
            public double Capacity { get; set; }
            public Func<Random, ILoad, TimeSpan> ServiceTime { get; set; }
        }

        #region Dynamic Properties
        public double Capacity { get { return Assets.Capacity; } }
        public int Occupancy { get { return HSet_Serving.Count + HSet_PendingToDepart.Count; } }
        public double Vacancy { get { return Capacity - Occupancy; } }
        public double AvgNServing { get { return HC_Serving.AverageCount; } }
        public double AvgNOccupying { get { return HC_Serving.AverageCount + HC_PendingToDepart.AverageCount; } }
        public double UtilServing { get { return AvgNServing / Capacity; } }
        public double UtilOccupying { get { return AvgNOccupying / Capacity; } }
        public IReadOnlyList<ILoad> PendingToStart { get { return List_PendingToStart.AsReadOnly(); } }
        public IReadOnlyList<ILoad> Serving { get { return HSet_Serving.ToList().AsReadOnly(); } }
        public IReadOnlyList<ILoad> PendingToDepart { get { return HSet_PendingToDepart.ToList().AsReadOnly(); } }

        private HourCounter HC_Serving { get; set; }
        private HourCounter HC_PendingToDepart { get; set; }
        private readonly List<ILoad> List_PendingToStart = new List<ILoad>();
        private readonly HashSet<ILoad> HSet_Serving = new HashSet<ILoad>();
        private readonly HashSet<ILoad> HSet_PendingToDepart = new HashSet<ILoad>();
        #endregion

        #region Events
        public void RqstStart(ILoad load)
        {
            Log("Request to Start", load);
            if (DebugMode) Debug.WriteLine("{0}:\t{1}\tRqstStart\t{2}", ClockTime, this, load);
            List_PendingToStart.Add(load);
            AtmptStart();
        }

        private void AtmptStart()
        {
            if (List_PendingToStart.Count > 0 && Vacancy > 0)
            {
                var load = List_PendingToStart.First();
                Log("Start", load);
                if (DebugMode) Debug.WriteLine("{0}:\t{1}\tStart\t{2}", ClockTime, this, load);
                List_PendingToStart.RemoveAt(0);
                HSet_Serving.Add(load);
                HC_Serving.ObserveChange(1, ClockTime);
                OnStarted.Invoke(load);
                Schedule(() => ReadyToDepart(load), Assets.ServiceTime(DefaultRS, load));
            }
        }

        private void ReadyToDepart(ILoad load)
        {
            Log("Ready to Depart", load);
            if (DebugMode) Debug.WriteLine("{0}:\t{1}\tReadyToDepart\t{2}", ClockTime, this, load);
            HSet_Serving.Remove(load);
            HSet_PendingToDepart.Add(load);
            HC_Serving.ObserveChange(-1, ClockTime);
            HC_PendingToDepart.ObserveChange(1, ClockTime);
            OnReadyToDepart.Invoke(load);
        }

        public void Depart(ILoad load)
        {
            if (HSet_PendingToDepart.Contains(load))
            {
                Log("Depart", load);
                if (DebugMode) Debug.WriteLine("{0}:\t{1}\tDepart\t{2}", ClockTime, this, load);
                HSet_PendingToDepart.Remove(load);
                HC_PendingToDepart.ObserveChange(-1, ClockTime);
                AtmptStart();
            }
        }

        public event Action<ILoad> OnStarted = Load => { };
        public event Action<ILoad> OnReadyToDepart = load => { };
        #endregion

        public Server(Statics assets, int seed = 0, string id = null)
            : base(assets, seed, id)
        {
            HC_Serving = AddHourCounter();
            HC_PendingToDepart = AddHourCounter();
        }

        public override void Dispose()
        {
            foreach (Action<ILoad> i in OnStarted.GetInvocationList()) OnStarted -= i;
            foreach (Action<ILoad> i in OnReadyToDepart.GetInvocationList()) OnReadyToDepart -= i;
        }
    }
}
