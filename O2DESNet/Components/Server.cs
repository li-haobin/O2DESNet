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
        #endregion

        #region Dynamic Properties
        public HashSet<TLoad> Serving { get; private set; }
        public int Vancancy { get { return Capacity - Serving.Count; } }
                
        public HourCounter HourCounter { get; private set; } // statistics
        internal Random RS { get; private set; } // random stream
        #endregion

        #region Input Events - Generators
        public Event<TScenario, TStatus> Start(TLoad load) { return new StartEvent(this, load); }
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
            public override string ToString() { return "Server_Start"; }
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
                Server.HourCounter.ObserveChange(-1, ClockTime);
                foreach (var evnt in Server.OnFinish) Execute(evnt(Load));
            }
            public override string ToString() { return "Server_Finish"; }
        }
        #endregion

        #region Output Events - Reference to Event Generators
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnFinish { get; set; }
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
        #endregion

        public Server(int capacity = int.MaxValue, int seed = -1)
        {
            Capacity = capacity;
            Serving = new HashSet<TLoad>();
            RS = seed < 0 ? null : new Random(seed);
            HourCounter = new HourCounter(DateTime.MinValue);
            OnFinish = new List<Func<TLoad, Event<TScenario, TStatus>>>();
        }

        public void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }

    }
}
