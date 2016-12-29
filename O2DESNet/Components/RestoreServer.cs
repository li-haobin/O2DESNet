using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class RestoreServer<TLoad> : Component<RestoreServer<TLoad>.Statics>
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
        public int Vancancy { get; private set; } = int.MaxValue;
        public int NCompleted { get { return (int)H_Server.HourCounter.TotalDecrementCount; } }
        public int NOccupied { get; private set; } = 0;
        public double Utilization { get { return (H_Server.HourCounter.AverageCount + R_Server.HourCounter.AverageCount) / Config.Capacity; } }
        public double EffectiveHourlyRate { get { return H_Server.HourCounter.DecrementRate; } }
        public bool ToDepart { get { return H_Server.ToDepart; } }
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
                Execute(RestoreServer.H_Server.Start(Load));
                Execute(new UpdateVacancyEvent(RestoreServer));
            }
            public override string ToString() { return string.Format("{0}_Start", RestoreServer); }
        }
        private class UpdateVacancyEvent : Event
        {
            public RestoreServer<TLoad> RestoreServer { get; private set; }
            internal UpdateVacancyEvent(RestoreServer<TLoad> restoreServer) { RestoreServer = restoreServer; }
            public override void Invoke()
            {
                bool hadVacancy = RestoreServer.Vancancy > 0;

                RestoreServer.NOccupied = RestoreServer.Serving.Count + RestoreServer.Served.Count + RestoreServer.Restoring.Count;
                RestoreServer.Vancancy = RestoreServer.Config.Capacity - RestoreServer.NOccupied;
                if (hadVacancy && RestoreServer.Vancancy == 0) foreach (var evnt in RestoreServer.OnReady) Execute(evnt(false));
                if (!hadVacancy && RestoreServer.Vancancy > 0) foreach (var evnt in RestoreServer.OnReady) Execute(evnt(true));
            }
            public override string ToString() { return string.Format("{0}_UpdateVacancy", RestoreServer); }
        }
        #endregion

        #region Input Events - Getters
        public Event Start(TLoad load)
        {            
            if (Config.HandlingTime == null) throw new HandlingTimeNotSpecifiedException();
            if (Config.RestoringTime == null) throw new RestoringTimeNotSpecifiedException();
            return new StartEvent(this, load);
        }
        public Event UpdateToDepart(bool toDepart) { return H_Server.UpdateToDepart(toDepart); }
        #endregion
               
        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event>> OnDepart { get { return H_Server.OnDepart; } }
        public List<Func<TLoad, Event>> OnRestore { get { return R_Server.OnDepart; } }
        public List<Func<bool, Event>> OnReady { get { return H_Server.OnReady; } }
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
        #endregion

        public RestoreServer(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "RestoreServer";
            H_Server = new Server<TLoad>(config.H_Server, DefaultRS.Next());
            R_Server = new Server<TLoad>(config.R_Server, DefaultRS.Next());

            // connect sub-components
            H_Server.OnDepart.Add(R_Server.Start);
            OnRestore.Add(load => new UpdateVacancyEvent(this));
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
