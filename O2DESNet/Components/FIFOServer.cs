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
        public HashSet<TLoad> Serving { get { return InnerServer.Serving; } }
        public HashSet<TLoad> Served { get { return InnerServer.Served; } }
        public int Vancancy { get { return InnerServer.Vancancy; } }
        public List<TLoad> Sequence { get; private set; }
        public HourCounter HourCounter { get { return InnerServer.HourCounter; } } // statistics   
        public int NCompleted { get { return (int)HourCounter.TotalDecrementCount; } }
        public double Utilization { get { return InnerServer.Utilization; } }        
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
            public override void Invoke() { FIFOServer.Sequence.RemoveAt(0); }
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
                    ToDepart = load => Config.ToDepart(load) && load.Equals(Sequence.First())
                },
                DefaultRS.Next(),
                tag: "InnerServer");
            InnerServer.OnDepart.Add(l => new RemoveEvent(this));
        }

        public override void WarmedUp(DateTime clockTime) { InnerServer.WarmedUp(clockTime); }

        public override void WriteToConsole() { InnerServer.WriteToConsole(); }
    }
}
