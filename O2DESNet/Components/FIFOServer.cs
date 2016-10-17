using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet
{
    public class FIFOServer<TLoad> : Component<FIFOServer<TLoad>.Statics>
        where TLoad : Load
    {
        #region Statics
        public class Statics : Server<TLoad>.Statics { }
        #endregion

        #region Sub-Components
        internal Server<TLoad> InnerServer { get; private set; }
        #endregion

        #region Dynamics
        /// <summary>
        /// Loads that the server is currently serving (including those being delayed).
        /// </summary>
        public HashSet<TLoad> Serving
        {
            get
            {
                var serving = new HashSet<TLoad>();
                int i = 0;
                while (i < Sequence.Count)
                {
                    if (!InnerServer.Served.Contains(Sequence[i])) break;
                    i++;
                }
                while (i < Sequence.Count)
                {
                    serving.Add(Sequence[i]);
                    i++;
                }
                return serving;
            }
        }
        /// <summary>
        /// Loads that have been served by the server.
        /// </summary>
        public HashSet<TLoad> Served
        {
            get
            {
                var served = new HashSet<TLoad>();
                int i = 0;
                while (i < Sequence.Count)
                {
                    if (!InnerServer.Served.Contains(Sequence[i])) break;
                    served.Add(Sequence[i]);
                    i++;
                }
                return served;
            }
        }
        /// <summary>
        /// Loads that the server is currently but delayed due to FIFO rule.
        /// </summary>
        public HashSet<TLoad> Delayed
        {
            get
            {
                var delayed = new HashSet<TLoad>();

                int i = 0;
                while (i < Sequence.Count)
                {
                    if (!InnerServer.Served.Contains(Sequence[i])) break;
                    i++;
                }
                while (i < Sequence.Count)
                {
                    if (InnerServer.Served.Contains(Sequence[i])) delayed.Add(Sequence[i]);
                    i++;
                }
                return delayed;
            }
        }

        public int Vancancy { get { return InnerServer.Vancancy; } }
        public List<TLoad> Sequence { get; private set; }
        public HourCounter HourCounter { get { return InnerServer.HourCounter; } } // statistics   
        public int NCompleted { get { return (int)HourCounter.TotalDecrementCount; } }
        public int NOccupied { get { return InnerServer.NOccupied; } }
        public double Utilization { get { return InnerServer.Utilization; } }
        public Dictionary<TLoad, DateTime> StartTimes { get { return InnerServer.StartTimes; } }

        /// <summary>
        /// The finish time of the previously departed Load
        /// </summary>
        private DateTime PrevFinishTime = DateTime.MinValue;
        /// <summary>
        /// The finish time of the last served load, return null if there is no served load
        /// </summary>
        public DateTime? FinishTime
        {
            get
            {
                if (Served.Count == 0) return null;
                // note that the finish time of inner server is NOT the true finish time as it should be delayed by previously departed load
                var finishTime = InnerServer.FinishTimes[Sequence.First()]; 
                return finishTime > PrevFinishTime ? finishTime : PrevFinishTime;
            }
        }
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
                FIFOServer.Sequence.Add(Load);
                Execute(FIFOServer.InnerServer.Start(Load));                
            }
            public override string ToString() { return string.Format("{0}_Start", FIFOServer); }
        }
        private class RemoveEvent : Event
        {
            public FIFOServer<TLoad> FIFOServer { get; private set; }
            internal RemoveEvent(FIFOServer<TLoad> fifoServer) { FIFOServer = fifoServer; }
            public override void Invoke()
            {
                var finishTime = FIFOServer.InnerServer.FinishTimes[FIFOServer.Sequence.First()];
                if (finishTime > FIFOServer.PrevFinishTime) FIFOServer.PrevFinishTime = finishTime;
                FIFOServer.Sequence.RemoveAt(0);
            }
            public override string ToString() { return string.Format("{0}_Remove", FIFOServer); }
        }
        #endregion

        #region Input Events - Getters
        public Event Start(TLoad load) { return new StartEvent(this, load); }
        public Event Depart() { return InnerServer.Depart(); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event>> OnDepart { get { return InnerServer.OnDepart; } }
        #endregion

        #region Exeptions
        #endregion

        public FIFOServer(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "FIFOServer";
            Sequence = new List<TLoad>();

            // connect sub-components
            InnerServer = new Server<TLoad>(
                new Server<TLoad>.Statics
                {
                    Capacity = Config.Capacity,
                    ServiceTime = Config.ServiceTime,
                    ToDepart = load => Config.ToDepart(load) && load == Sequence.First(),
                },
                DefaultRS.Next(),
                tag: "InnerServer");
            InnerServer.OnDepart.Add(l => new RemoveEvent(this));
        }

        public override void WarmedUp(DateTime clockTime) { InnerServer.WarmedUp(clockTime); }

        public override void WriteToConsole(DateTime? clockTime = null) { InnerServer.WriteToConsole(); }
    }
}
