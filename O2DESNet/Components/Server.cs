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
        where TLoad : Load
    {
        public int Capacity { get; private set; }
        public HashSet<TLoad> Serving { get; private set; }
        public int Vacancy { get { return Capacity - Serving.Count; } }
        public bool HasVacancy() { return Vacancy > 0; }        
        public Func<Random, TimeSpan> ServiceTime { get; set; }
        public Action<TLoad> OnFinish { get; set; }
        public HourCounter HourCounter { get; private set; }

        public Server(int capacity = int.MaxValue)
        {
            Capacity = capacity;
            Serving = new HashSet<TLoad>();
            HourCounter = new HourCounter(DateTime.MinValue);
        }

        public Start StartEvent(TLoad load) { return new Start(this, load); }

        public void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }
        
        public class Start : Event<TScenario, TStatus>
        {
            public Server<TScenario, TStatus, TLoad> Server { get; private set; }
            public TLoad Load { get; private set; }

            internal Start(Server<TScenario, TStatus, TLoad> server, TLoad load)
            {
                Server = server;
                Load = load;
            }

            public override void Invoke()
            {
                Log("{0} {1} starts service.", ClockTime.ToLongTimeString(), Load);
                if (Load.TimeStamp_StartProcess == null) Load.TimeStamp_StartProcess = ClockTime;

                if (!Server.HasVacancy()) throw new Exception("The Processor does not have vacancy.");
                Server.Serving.Add(Load);
                Server.HourCounter.ObserveChange(1, ClockTime);

                Schedule(new Finish(Server, Load), Server.ServiceTime(DefaultRS));
            }
        }

        public class Finish : Event<TScenario, TStatus>
        {
            public Server<TScenario, TStatus, TLoad> Server { get; private set; }
            public TLoad Load { get; private set; }

            internal Finish(Server<TScenario, TStatus, TLoad> server, TLoad load)
            {
                Server = server;
                Load = load;
            }

            public override void Invoke()
            {
                Log("{0} {1} finishes service.", ClockTime.ToLongTimeString(), Load);
                if (Load.TimeStamp_FinishProcess == null) Load.TimeStamp_FinishProcess = ClockTime;

                if (!Server.Serving.Contains(Load)) throw new Exception("The Processor is not processing the Load.");
                Server.Serving.Remove(Load);
                Server.HourCounter.ObserveChange(-1, ClockTime);

                Server.OnFinish(Load);
            }
        }

    }
}
