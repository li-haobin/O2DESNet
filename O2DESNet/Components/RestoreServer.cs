using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class RestoreServer<TLoad> : Component<RestoreServer<TLoad>.Statics>
        where TLoad : Load
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
            //public Func<TLoad, bool> ToDepart { get { return H_Server.ToDepart; } set { H_Server.ToDepart = value; } }
            public int Capacity { get; set; }
        }
        #endregion

        #region Dynamics
        public HashSet<TLoad> Serving { get { return H_Server.Serving; } }
        public HashSet<TLoad> Served { get { return H_Server.Served; } }
        public HashSet<TLoad> Restoring { get { return R_Server.Serving; } }
        public int Vancancy { get { return Config.Capacity - NOccupied; } }
        public int NCompleted { get { return (int)H_Server.HourCounter.TotalDecrementCount; } }
        public int NOccupied { get { return Serving.Count + Served.Count + Restoring.Count; } }
        public double Utilization { get { return (H_Server.HourCounter.AverageCount + R_Server.HourCounter.AverageCount) / Config.Capacity; } }
        public double EffectiveHourlyRate { get { return H_Server.HourCounter.DecrementRate; } }
        #endregion

        #region Events
        private class StartEvent : Event
        {
            public RestoreServer<TLoad> RestoreServer { get; private set; }
            public TLoad Load { get; private set; }
            internal StartEvent(RestoreServer<TLoad> restoreServer, TLoad load)
            {
                RestoreServer = restoreServer;
                Load = load;
            }
            public override void Invoke()
            {
                if (RestoreServer.Vancancy < 1) throw new HasZeroVacancyException();
                Load.Log(this);
                Execute(RestoreServer.H_Server.Start(Load));
            }
            public override string ToString() { return string.Format("{0}_Start", RestoreServer); }
        }
        private class RestoreEvent : Event
        {
            public RestoreServer<TLoad> RestoreServer { get; private set; }
            public TLoad Load { get; private set; }
            internal RestoreEvent(RestoreServer<TLoad> restoreServer, TLoad load)
            {
                RestoreServer = restoreServer;
                Load = load;
            }
            public override void Invoke()
            {
                Load.Log(this);
                foreach (var evnt in RestoreServer.OnRestore) Execute(evnt());
            }
            public override string ToString() { return string.Format("{0}_Restore", RestoreServer); }
        }
        #endregion

        #region Input Events - Getters
        //public Event Depart() { return H_Server.Depart(); }
        public Event Start(TLoad load)
        {            
            if (Config.HandlingTime == null) throw new HandlingTimeNotSpecifiedException();
            if (Config.RestoringTime == null) throw new RestoringTimeNotSpecifiedException();
            //if (Config.ToDepart == null) throw new DepartConditionNotSpecifiedException();
            return new StartEvent(this, load);
        }
        #endregion
               
        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event>> OnDepart { get { return H_Server.OnDepart; } }
        public List<Func<Event>> OnRestore { get; private set; }
        #endregion

        #region Exeptions
        public class HasZeroVacancyException : Exception
        {
            public HasZeroVacancyException() : base("Check vacancy of the Server before execute Start event.") { }
        }
        public class HandlingTimeNotSpecifiedException : Exception
        {
            public HandlingTimeNotSpecifiedException() : base("Set HandlingTime as a random generator.") { }
        }
        public class RestoringTimeNotSpecifiedException : Exception
        {
            public RestoringTimeNotSpecifiedException() : base("Set RestoringTime as a random generator.") { }
        }
        public class DepartConditionNotSpecifiedException : Exception
        {
            public DepartConditionNotSpecifiedException() : base("Set ToDepart as depart condition.") { }
        }
        #endregion

        public RestoreServer(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "RestoreServer";
            H_Server = new Server<TLoad>(config.H_Server, DefaultRS.Next());
            R_Server = new Server<TLoad>(config.R_Server, DefaultRS.Next());

            // connect sub-components
            H_Server.OnDepart.Add(R_Server.Start);
            //R_Server.Config.ToDepart = (load) => true;
            R_Server.OnDepart.Add(l => new RestoreEvent(this, l));

            // initialize for output events
            OnRestore = new List<Func<Event>>();
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
