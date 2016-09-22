using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Server<TScenario, TStatus, TLoad> : Component
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Statics
        public class StaticProperties : Scenario
        {
            /// <summary>
            /// Maximum number of loads in the queue
            /// </summary>
            public int Capacity { get; set; } = int.MaxValue;
            /// <summary>
            /// Random generator for service time
            /// </summary>
            public Func<TLoad, Random, TimeSpan> ServiceTime { get; set; }
            public Func<TLoad, bool> ToDepart { get; set; }
        }
        public StaticProperties Statics { get; private set; }
        #endregion

        #region Dynamics
        public HashSet<TLoad> Serving { get; private set; }
        public List<TLoad> Served { get; private set; }
        public int Vancancy { get { return Statics.Capacity - Serving.Count - Served.Count; } }
        public HourCounter HourCounter { get; private set; } // statistics        
        #endregion

        #region Events
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
                if (Server.Vancancy < 1) throw new HasZeroVacancyException();
                Load.Log(this);
                Server.Serving.Add(Load);
                Server.HourCounter.ObserveChange(1, ClockTime);
                Schedule(new FinishEvent(Server, Load), Server.Statics.ServiceTime(Load, Server.DefaultRS));
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
                if (Server.Statics.ToDepart == null) throw new DepartConditionNotSpecifiedException();
                var load = Server.Served.FirstOrDefault();
                if (load == null) return;
                if (Server.Statics.ToDepart(load))
                {                    
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

        #region Input Events - Getters
        public Event<TScenario, TStatus> Start(TLoad load) { return new StartEvent(this, load); }
        public Event<TScenario, TStatus> Depart() { return new DepartEvent(this); }        
        #endregion

        #region Output Events - Reference to Getters
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
        
        public Server(StaticProperties statics, int seed, string tag = null) : base(seed, tag)
        {
            Name = "Server";
            Statics = statics;
            Serving = new HashSet<TLoad>();
            Served = new List<TLoad>();
            HourCounter = new HourCounter();

            // initialize for output events    
            OnDepart = new List<Func<TLoad, Event<TScenario, TStatus>>>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            HourCounter.WarmedUp(clockTime);
        }

        public override void WriteToConsole()
        {
            Console.WriteLine("[{0}]", this);
            Console.Write("Serving: ");
            foreach (var load in Serving) Console.Write("{0} ", load);
            Console.WriteLine();
            Console.Write("Served: ");
            foreach (var load in Served) Console.Write("{0} ", load);
            Console.WriteLine();
        }
    }
}
