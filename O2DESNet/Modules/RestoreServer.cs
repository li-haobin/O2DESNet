using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class RestoreServer<TLoad> : State<RestoreServer<TLoad>.Statics>
    {
        #region Sub-Components
        internal Server<TLoad> H_Server { get; private set; }
        internal Server<TLoad> R_Server { get; private set; }
        #endregion

        #region Statics
        public class Statics : Scenario
        {
            internal Server<TLoad>.Statics H_Server { get; private set; } = new Server<TLoad>.Statics();
            internal Server<TLoad>.Statics R_Server { get; private set; } = new Server<TLoad>.Statics();

            public Func<TLoad, Random, TimeSpan> HandlingTime { get { return H_Server.ServiceTime; } set { H_Server.ServiceTime = value; } }
            public Func<TLoad, Random, TimeSpan> RestoringTime { get { return R_Server.ServiceTime; } set { R_Server.ServiceTime = value; } }
            public int Capacity { get; set; }
        }
        #endregion

        #region Dynamics
        public HashSet<TLoad> Serving { get { return H_Server.Serving; } }
        public HashSet<TLoad> Served { get { return H_Server.Served; } }
        public HashSet<TLoad> Restoring { get { return R_Server.Serving; } }
        public int Vacancy { get { return Config.Capacity - Occupancy; } }
        public int NCompleted { get { return (int)H_Server.UtilizationCounter.TotalDecrementCount; } }
        public int Occupancy { get { return Serving.Count + Served.Count + Restoring.Count; } }
        public double Utilization { get { return (H_Server.UtilizationCounter.AverageCount + R_Server.UtilizationCounter.AverageCount) / Config.Capacity; } }
        public double Occupation { get { return (H_Server.OccupationCounter.AverageCount + R_Server.OccupationCounter.AverageCount) / Config.Capacity; } }
        public double EffectiveHourlyRate { get { return H_Server.UtilizationCounter.DecrementRate; } }
        public bool ToDepart { get { return H_Server.ToDepart; } }
        #endregion

        #region Events
        private abstract class InternalEvent : Event<RestoreServer<TLoad>, Statics> { }
        private class StartEvent : InternalEvent
        {
            internal TLoad Load { get; set; }
            public override void Invoke()
            {
                if (This.Vacancy < 1) throw new HasZeroVacancyException();
                Execute(This.H_Server.Start(Load));
                Execute(new StateChgEvent());
            }
            public override string ToString() { return string.Format("{0}_Start", This); }
        }
        private class StateChgEvent : InternalEvent
        {
            public override void Invoke() { Execute(This.OnStateChg, e => e()); }
            public override string ToString() { return string.Format("{0}_StateChange", This); }
        }
        #endregion

        #region Input Events - Getters
        public Event Start(TLoad load) { return new StartEvent { This = this, Load = load }; }
        public Event UpdToDepart(bool toDepart) { return H_Server.UpdToDepart(toDepart); }
        #endregion
               
        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event>> OnDepart { get { return H_Server.OnDepart; } }
        public List<Func<TLoad, Event>> OnRestore { get { return R_Server.OnDepart; } }
        public List<Func<Event>> OnStateChg { get; private set; } = new List<Func<Event>>();
        #endregion

        #region Exeptions
        public class HasZeroVacancyException : Exception
        {
            public HasZeroVacancyException() : base("Check vacancy of the Server before execute Start event.") { }
        }
        #endregion

        public RestoreServer(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "RestoreServer";
            H_Server = new Server<TLoad>(config.H_Server, DefaultRS.Next());
            R_Server = new Server<TLoad>(config.R_Server, DefaultRS.Next());

            // connect sub-components
            H_Server.OnDepart.Add(R_Server.Start);
            H_Server.OnStateChg.Add(() => new StateChgEvent { This = this });
            R_Server.OnStateChg.Add(() => new StateChgEvent { This = this });
        }

        public override void WarmedUp(DateTime clockTime)
        {
            H_Server.WarmedUp(clockTime);
            R_Server.WarmedUp(clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime = null)
        {
            Console.WriteLine("[{0}]", this);
            Console.Write("Serving: ");
            foreach (var load in Serving) Console.Write("{0} ", load);
            Console.WriteLine();
            Console.Write("Served: ");
            foreach (var load in Served) Console.Write("{0} ", load);
            Console.WriteLine();
            Console.Write("Restoring: ");
            foreach (var load in Restoring) Console.Write("{0} ", load);
            Console.WriteLine();
        }
    }
}
