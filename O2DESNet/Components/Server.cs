using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Server<TScenario, TStatus, TLoad>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Static Properties
        /// <summary>
        /// Maximum number of loads in the queue
        /// </summary>
        public int Capacity { get; set; }
        /// <summary>
        /// Random generator for service time
        /// </summary>
        public Func<Random, TimeSpan> ServiceTime { get; set; }
        public Func<bool> ToDepart { get; set; }
        #endregion

        #region Dynamic Properties
        public HashSet<TLoad> Serving { get; private set; }
        public List<TLoad> Served { get; private set; }
        public int Vancancy { get { return Capacity - Serving.Count; } }
                
        public HourCounter HourCounter { get; private set; } // statistics
        internal Random RS { get; private set; } // random stream
        #endregion

        #region Input Events - Generators
        public Event<TScenario, TStatus> Start(TLoad load) { return new StartEvent(this, load); }
        public Event<TScenario, TStatus> Depart() { return new DepartEvent(this); }
        private class StartEvent : Event<TScenario, TStatus>
        {
            public Server<TScenario, TStatus, TLoad> Server { get; private set; }
            public TLoad Load { get; private set; }
            internal StartEvent(Server<TScenario, TStatus, TLoad> server, TLoad load)
            {
                Server = server;
                Load = load;
            }
            public override void Invoke()
            {
                if (Server.Vancancy == 0) throw new HasZeroVacancyException();
                Load.Log(this);
                Server.Serving.Add(Load);
                Server.HourCounter.ObserveChange(1, ClockTime);
                Schedule(new FinishEvent(Server, Load), Server.ServiceTime(Server.RS == null ? DefaultRS : Server.RS));
            }
            public override string ToString() { return string.Format("{0}_Start", Server); }
        }
        private class FinishEvent : Event<TScenario, TStatus>
        {
            public Server<TScenario, TStatus, TLoad> Server { get; private set; }
            public TLoad Load { get; private set; }
            internal FinishEvent(Server<TScenario, TStatus, TLoad> server, TLoad load)
            {
                Server = server;
                Load = load;
            }
            public override void Invoke()
            {
                Load.Log(this);
                Server.Serving.Remove(Load);
                Server.Served.Add(Load);
                Execute(new DepartEvent(Server));
            }
            public override string ToString() { return string.Format("{0}_Finish", Server); }
        }
        private class DepartEvent : Event<TScenario, TStatus>
        {
            public Server<TScenario, TStatus, TLoad> Server { get; private set; }
            internal DepartEvent(Server<TScenario, TStatus, TLoad> server)
            {
                Server = server;
            }
            public override void Invoke()
            {
                if (Server.ToDepart == null) throw new DepartConditionNotSpecifiedException();
                if (Server.ToDepart())
                {
                    var load = Server.Served.FirstOrDefault();
                    if (load == null) return;
                    load.Log(this);
                    Server.Served.RemoveAt(0);
                    Server.HourCounter.ObserveChange(-1, ClockTime);
                    foreach (var evnt in Server.OnDepart) Execute(evnt(load));
                    Execute(new DepartEvent(Server));
                }                
            }
            public override string ToString() { return string.Format("{0}_Depart", Server); }
        }
        #endregion

        #region Output Events - Reference to Event Generators
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnDepart { get; private set; }
        #endregion
        
        #region Exeptions
        public class HasZeroVacancyException : Exception
        {
            public HasZeroVacancyException() : base("Check vacancy of the Server before execute Start event.") { }
        }
        public class ServiceTimeNotSpecifiedException : Exception
        {
            public ServiceTimeNotSpecifiedException() : base("Set ServiceTime as a random generator.") { }
        }
        public class DepartConditionNotSpecifiedException : Exception
        {
            public DepartConditionNotSpecifiedException() : base("Set ToDepart as depart condition.") { }
        }
        #endregion

        private static int _count = 0;
        public int Id { get; private set; }
        public Server(int capacity = int.MaxValue, int seed = -1)
        {
            Id = _count++;
            Capacity = capacity;
            Serving = new HashSet<TLoad>();
            Served = new List<TLoad>();
            RS = seed < 0 ? null : new Random(seed);
            HourCounter = new HourCounter(DateTime.MinValue);
            OnDepart = new List<Func<TLoad, Event<TScenario, TStatus>>>();
        }
        public void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }
        public override string ToString() { return string.Format("Server#{0}", Id); }

    }
}
