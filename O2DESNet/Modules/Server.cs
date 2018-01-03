using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Server<TLoad> : State<Server<TLoad>.Statics>
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
        private abstract class InternalEvent : Event<Server<TLoad>, Statics> { }
        private class StartEvent : InternalEvent
        {
            internal TLoad Load { get; set; }
            public override void Invoke()
            {
                if (This.Vacancy < 1) throw new HasZeroVacancyException();
                This.PushIn(Load);
                This.UtilizationCounter.ObserveChange(1, ClockTime);
                This.OccupationCounter.ObserveChange(1, ClockTime);
                This.StartTimes.Add(Load, ClockTime);
                Schedule(new FinishEvent { Load = Load }, Config.ServiceTime(Load, DefaultRS));
                Execute(new StateChgEvent());
            }
            public override string ToString() { return string.Format("{0}_Start", This); }
        }
        private class FinishEvent : InternalEvent
        {
            internal TLoad Load { get; set; }
            public override void Invoke()
            {
                This.Serving.Remove(Load);
                This.Served.Add(Load);
                This.FinishTimes.Add(Load, ClockTime);
                This.UtilizationCounter.ObserveChange(-1, ClockTime);
                Execute(new StateChgEvent());
                if (This.ChkToDepart()) Execute(new DepartEvent());                
            }
            public override string ToString() { return string.Format("{0}_Finish", This); }
        }
        private class UpdToDepartEvent : InternalEvent
        {
            internal bool ToDepart { get; set; }
            public override void Invoke()
            {
                This.ToDepart = ToDepart;
                if (This.ChkToDepart()) Execute(new DepartEvent());
            }
            public override string ToString() { return string.Format("{0}_UpdToDepart", This); }
        }
        private class StateChgEvent : InternalEvent
        {
            public override void Invoke() { Execute(This.OnStateChg.Select(e => e())); }
            public override string ToString() { return string.Format("{0}_StateChange", This); }
        }
        private class DepartEvent : InternalEvent
        {
            public override void Invoke()
            {
                TLoad load = This.GetToDepart();
                This.Served.Remove(load);
                This.OccupationCounter.ObserveChange(-1, ClockTime);
                // in case the start / finish times are used in OnDepart events
                This.StartTimes.Remove(load);
                This.FinishTimes.Remove(load);
                foreach (var evnt in This.OnDepart) Execute(evnt(load));

                Execute(new StateChgEvent());
                if (This.ChkToDepart()) Execute(new DepartEvent());                
            }
            public override string ToString() { return string.Format("{0}_Depart", This); }
        }
        #endregion

        #region Input Events - Getters
        public Event Start(TLoad load) { return new StartEvent { This = this, Load = load }; }
        public Event UpdToDepart(bool toDepart) { return new UpdToDepartEvent { This = this, ToDepart = toDepart }; }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event>> OnDepart { get; private set; } = new List<Func<TLoad, Event>>();
        public List<Func<Event>> OnStateChg { get; private set; } = new List<Func<Event>>();
        #endregion

        #region Exceptions
        public class HasZeroVacancyException : Exception
        {
            public HasZeroVacancyException() : base("Check vacancy of the Server before execute Start event.") { }
        }
        public class ServiceTimeNotSpecifiedException : Exception
        {
            public ServiceTimeNotSpecifiedException() : base("Set ServiceTime as a random generator.") { }
        }
        #endregion

        public Server(Statics config, int seed, string tag = null) : base(config, seed, tag) { Name = "Server"; }

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