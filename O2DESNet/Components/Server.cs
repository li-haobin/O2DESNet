using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Server<TLoad> : Component<Server<TLoad>.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            /// <summary>
            /// Maximum number of loads in the queue
            /// </summary>
            public int Capacity { get; set; } = int.MaxValue;
            /// <summary>
            /// Random generator for service time
            /// </summary>
            public Func<TLoad, Random, TimeSpan> ServiceTime { get; set; }
        }
        #endregion

        #region Dynamics
        public HashSet<TLoad> Serving { get; private set; } = new HashSet<TLoad>();
        public HashSet<TLoad> Served { get; private set; } = new HashSet<TLoad>();
        public int Vacancy { get { return Config.Capacity - Occupancy; } }
        public int Occupancy { get { return Serving.Count + Served.Count; } }
        public HourCounter UtilizationCounter { get; private set; } = new HourCounter(); // for utilization    
        public HourCounter OccupationCounter { get; private set; } = new HourCounter(); // for occupation
        /// <summary>
        /// Number of loads processed by the server, including those have not departed
        /// </summary>
        public int NCompleted { get { return (int)UtilizationCounter.TotalDecrementCount; } }
        public double Utilization { get { return UtilizationCounter.AverageCount / Config.Capacity; } }
        public double Occupation { get { return OccupationCounter.AverageCount / Config.Capacity; } }
        public Dictionary<TLoad, DateTime> StartTimes { get; private set; } = new Dictionary<TLoad, DateTime>();
        public Dictionary<TLoad, DateTime> FinishTimes { get; private set; } = new Dictionary<TLoad, DateTime>();
        public bool ToDepart { get; protected set; } = true;

        protected virtual void PushIn(TLoad load) { Serving.Add(load); }
        protected virtual bool ChkToDepart() { return ToDepart && Served.Count > 0; }
        protected virtual TLoad GetToDepart() { return Served.FirstOrDefault(); }
        #endregion

        #region Events
        protected class StartEvent : Event
        {
            public Server<TLoad> Server { get; private set; }
            public TLoad Load { get; private set; }
            internal StartEvent(Server<TLoad> server, TLoad load)
            {
                Server = server;
                Load = load;
            }
            public override void Invoke()
            {
                if (Server.Vacancy < 1) throw new HasZeroVacancyException();
                Server.PushIn(Load);
                Server.UtilizationCounter.ObserveChange(1, ClockTime);
                Server.OccupationCounter.ObserveChange(1, ClockTime);
                Server.StartTimes.Add(Load, ClockTime);
                Schedule(new FinishEvent(Server, Load), Server.Config.ServiceTime(Load, Server.DefaultRS));
                Execute(new StateChgEvent(Server));
            }
            public override string ToString() { return string.Format("{0}_Start", Server); }
        }
        private class FinishEvent : Event
        {
            public Server<TLoad> Server { get; private set; }
            public TLoad Load { get; private set; }
            internal FinishEvent(Server<TLoad> server, TLoad load)
            {
                Server = server;
                Load = load;
            }
            public override void Invoke()
            {
                Server.Serving.Remove(Load);
                Server.Served.Add(Load);
                Server.FinishTimes.Add(Load, ClockTime);
                Server.UtilizationCounter.ObserveChange(-1, ClockTime);
                Execute(new StateChgEvent(Server));
                if (Server.ChkToDepart()) Execute(new DepartEvent(Server));                
            }
            public override string ToString() { return string.Format("{0}_Finish", Server); }
        }
        private class UpdToDepartEvent : Event
        {
            public Server<TLoad> Server { get; private set; }
            public bool ToDepart { get; private set; }

            internal UpdToDepartEvent(Server<TLoad> server, bool toDepart)
            {
                Server = server;
                ToDepart = toDepart;
            }
            public override void Invoke()
            {
                Server.ToDepart = ToDepart;
                if (Server.ChkToDepart()) Execute(new DepartEvent(Server));
            }
            public override string ToString() { return string.Format("{0}_UpdToDepart", Server); }
        }
        private class StateChgEvent : Event
        {
            public Server<TLoad> Server { get; private set; }
            internal StateChgEvent(Server<TLoad> server) { Server = server; }
            public override void Invoke()
            {
                foreach (var evnt in Server.OnStateChg) Execute(evnt(Server));
            }
            public override string ToString() { return string.Format("{0}_StateChange", Server); }
        }
        private class DepartEvent : Event
        {
            public Server<TLoad> Server { get; private set; }
            internal DepartEvent(Server<TLoad> server)
            {
                Server = server;
            }
            public override void Invoke()
            {
                TLoad load = Server.GetToDepart();
                Server.Served.Remove(load);
                Server.OccupationCounter.ObserveChange(-1, ClockTime);
                // in case the start / finish times are used in OnDepart events
                Server.StartTimes.Remove(load);
                Server.FinishTimes.Remove(load);
                foreach (var evnt in Server.OnDepart) Execute(evnt(load));

                Execute(new StateChgEvent(Server));
                if (Server.ChkToDepart()) Execute(new DepartEvent(Server));                
            }
            public override string ToString() { return string.Format("{0}_Depart", Server); }
        }
        #endregion

        #region Input Events - Getters
        public Event Start(TLoad load) { return new StartEvent(this, load); }
        public Event UpdToDepart(bool toDepart) { return new UpdToDepartEvent(this, toDepart); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event>> OnDepart { get; private set; } = new List<Func<TLoad, Event>>();
        public List<Func<Server<TLoad>, Event>> OnStateChg { get; private set; } = new List<Func<Server<TLoad>, Event>>();
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
        
        public Server(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Server";
        }

        public override void WarmedUp(DateTime clockTime)
        {
            UtilizationCounter.WarmedUp(clockTime);
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
        }
    }
}