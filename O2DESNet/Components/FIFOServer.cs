using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class FIFOServer<TLoad> : Component<FIFOServer<TLoad>.Statics>
        where TLoad : Load
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
            /// <summary>
            /// Random generator for the minimum time between two consecutive departures, given the two corresponding loads
            /// </summary>
            public Func<TLoad, TLoad, Random, TimeSpan> MinInterDepartureTime { get; set; }
            public Func<TLoad, bool> ToDepart { get; set; }
        }
        #endregion

        #region Dynamics
        public HashSet<TLoad> Serving { get; private set; }
        public HashSet<TLoad> Served { get; private set; }

        public int Vancancy { get { return Config.Capacity - NOccupied; } }
        public List<TLoad> Sequence { get; private set; } = new List<TLoad>();
        public HourCounter HourCounter { get; private set; } // statistics    
        public int NCompleted { get { return (int)HourCounter.TotalDecrementCount; } }
        public int NOccupied { get { return Serving.Count + Served.Count; } }
        public double Utilization { get { return HourCounter.AverageCount / Config.Capacity; } }
        public Dictionary<TLoad, DateTime> StartTimes { get; private set; }
        public Dictionary<TLoad, DateTime> FinishTimes { get; private set; }

        /// <summary>
        /// Time of being ready for departure
        /// </summary>
        public DateTime? ReadyTime { get; private set; } = null;

        /// <summary>
        /// Last departure time + inter-departure time
        /// </summary>
        public DateTime? NextAvailableDepartureTime { get; private set; } = null;
        public TLoad LastDepartedLoad { get; private set; } = null;
        public DateTime LastDepartureTime { get; private set; }
        #endregion

        #region Events
        private class StartEvent : Event
        {
            public FIFOServer<TLoad> FIFOServer { get; private set; }
            public TLoad Load { get; private set; }
            internal StartEvent(FIFOServer<TLoad> fifoServer, TLoad load)
            {
                FIFOServer = fifoServer;
                Load = load;
            }
            public override void Invoke()
            {
                if (FIFOServer.Vancancy < 1) throw new HasZeroVacancyException();
                Load.Log(this);
                FIFOServer.Sequence.Add(Load);
                FIFOServer.Serving.Add(Load);
                FIFOServer.HourCounter.ObserveChange(1, ClockTime);
                FIFOServer.StartTimes.Add(Load, ClockTime);
                Schedule(new FinishEvent(FIFOServer, Load), FIFOServer.Config.ServiceTime(Load, FIFOServer.DefaultRS));
            }
            public override string ToString() { return string.Format("{0}_Start", FIFOServer); }
        }
        private class FinishEvent : Event
        {
            public FIFOServer<TLoad> FIFOServer { get; private set; }
            public TLoad Load { get; private set; }
            internal FinishEvent(FIFOServer<TLoad> fifoServer, TLoad load)
            {
                FIFOServer = fifoServer;
                Load = load;
            }
            public override void Invoke()
            {
                Load.Log(this);
                FIFOServer.Serving.Remove(Load);
                FIFOServer.Served.Add(Load);
                FIFOServer.FinishTimes.Add(Load, ClockTime);
                Execute(new DepartEvent(FIFOServer));
            }
            public override string ToString() { return string.Format("{0}_Finish", FIFOServer); }
        }
        private class DepartEvent : Event
        {
            public FIFOServer<TLoad> FIFOServer { get; private set; }
            internal DepartEvent(FIFOServer<TLoad> fifoServer)
            {
                FIFOServer = fifoServer;
            }
            public override void Invoke()
            {
                if (FIFOServer.Config.ToDepart == null) throw new DepartConditionNotSpecifiedException();

                // there must be some load
                var load = FIFOServer.Sequence.FirstOrDefault();
                if (load == null) return;
                // the load must be served
                if (!FIFOServer.Served.Contains(load)) return;
                                
                if (FIFOServer.LastDepartedLoad != null)
                {
                    // must wait until next available departure time
                    if (FIFOServer.NextAvailableDepartureTime == null)
                    {
                        FIFOServer.NextAvailableDepartureTime = FIFOServer.LastDepartureTime + FIFOServer.Config.MinInterDepartureTime(FIFOServer.LastDepartedLoad, load, DefaultRS);
                        if (ClockTime < FIFOServer.NextAvailableDepartureTime)
                        {
                            Schedule(new DepartEvent(FIFOServer), FIFOServer.NextAvailableDepartureTime.Value);
                            return;
                        }
                    }
                    else if (ClockTime < FIFOServer.NextAvailableDepartureTime) return;
                }
                FIFOServer.ReadyTime = ClockTime;

                // the depart condition must be satisfied
                if (!FIFOServer.Config.ToDepart(load)) return;

                load.Log(this);
                FIFOServer.Served.Remove(load);
                FIFOServer.Sequence.RemoveAt(0);                  
                FIFOServer.HourCounter.ObserveChange(-1, ClockTime);                
                // in case the start / finish times are used in OnDepart events
                FIFOServer.StartTimes.Remove(load);
                FIFOServer.FinishTimes.Remove(load);

                FIFOServer.LastDepartedLoad = load;
                FIFOServer.LastDepartureTime = ClockTime;
                FIFOServer.NextAvailableDepartureTime = null;
                FIFOServer.ReadyTime = null;

                foreach (var evnt in FIFOServer.OnDepart) Execute(evnt(load));
                if (FIFOServer.Sequence.Count > 0) Execute(new DepartEvent(FIFOServer));                
            }
            public override string ToString() { return string.Format("{0}_Depart", FIFOServer); }
        }
        #endregion

        #region Input Events - Getters
        public Event Start(TLoad load) { return new StartEvent(this, load); }
        public Event Depart() { return new DepartEvent(this); }        
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event>> OnDepart { get; private set; }
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
        
        public FIFOServer(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "FIFOServer";
            Serving = new HashSet<TLoad>();
            Served = new HashSet<TLoad>();
            HourCounter = new HourCounter();
            StartTimes = new Dictionary<TLoad, DateTime>();
            FinishTimes = new Dictionary<TLoad, DateTime>();

            // initialize for output events    
            OnDepart = new List<Func<TLoad, Event>>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            HourCounter.WarmedUp(clockTime);
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
