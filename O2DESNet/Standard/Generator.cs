using System;
using System.Diagnostics;

namespace O2DESNet.Standard
{
    public class Generator : Sandbox<Generator.Statics>, IGenerator
    {
        public class Statics : IAssets
        {
            public string Id { get { return GetType().Name; } }
            public Func<Random, TimeSpan> InterArrivalTime { get; set; }
            public Generator Sandbox(int seed = 0) { return new Generator(this, seed); }
        }

        #region Dyanmic Properties
        public DateTime? StartTime { get; private set; }
        public bool IsOn { get; private set; }
        public int Count { get; private set; } // number of loads generated   
        #endregion

        #region Events
        public void Start()
        {
            if (!IsOn)
            {
                Log("Start");
                if (DebugMode) Debug.WriteLine("{0}:\t{1}\tStart", ClockTime, this);
                if (Assets.InterArrivalTime == null) throw new Exception("Inter-arrival time is null");
                IsOn = true;
                StartTime = ClockTime;
                Count = 0;
                ScheduleToArrive();
            }
        }

        public void End()
        {
            if (IsOn)
            {
                Log("End");
                if (DebugMode) Debug.WriteLine("{0}:\t{1}\tEnd", ClockTime, this);
                IsOn = false;
            }
        }

        private void ScheduleToArrive()
        {
            Schedule(Arrive, Assets.InterArrivalTime(DefaultRS));
        }

        private void Arrive()
        {
            if (IsOn)
            {
                Log("Arrive");
                if (DebugMode) Debug.WriteLine("{0}:\t{1}\tArrive", ClockTime, this);

                Count++;
                ScheduleToArrive();
                OnArrive.Invoke();
            }
        }

        public event Action OnArrive = () => { };
        #endregion

        public Generator(Statics assets, int seed = 0, string id = null)
            : base(assets, seed, id)
        {
            IsOn = false;
            Count = 0;
        }

        protected override void WarmedUpHandler()
        {
            Count = 0;
        }

        public override void Dispose()
        {
            foreach (Action i in OnArrive.GetInvocationList()) OnArrive -= i;
        }

    }
}
