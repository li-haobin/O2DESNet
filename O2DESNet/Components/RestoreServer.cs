using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class RestoreServer<TScenario, TStatus, TLoad>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Static Properties
        public Func<TLoad, Random, TimeSpan> HandlingTime { get { return H_Server.ServiceTime; } set { H_Server.ServiceTime = value; }  }
        public Func<TLoad, Random, TimeSpan> RestoringTime { get { return R_Server.ServiceTime; } set { R_Server.ServiceTime = value; } }
        public Func<bool> ToDepart { get { return H_Server.ToDepart; } set { H_Server.ToDepart = value; } }
        public int Capacity { get; set; }
        #endregion

        #region Dynamic Properties   
        public Server<TScenario, TStatus, TLoad> H_Server { get; private set; }
        public Server<TScenario, TStatus, TLoad> R_Server { get; private set; }
        public HashSet<TLoad> Serving { get { return H_Server.Serving; } }
        public List<TLoad> Served { get { return H_Server.Served; } }
        public HashSet<TLoad> Restoring { get { return R_Server.Serving; } }
        public int Vancancy { get { return Capacity - Serving.Count - Served.Count - Restoring.Count; } }
        public int NCompleted { get { return (int)H_Server.HourCounter.TotalDecrementCount; } }
        #endregion

        #region Input Events - Generators
        public Event<TScenario, TStatus> Start(TLoad load)
        {
            if (Vancancy < 1) throw new HasZeroVacancyException();
            if (HandlingTime == null) throw new HandlingTimeNotSpecifiedException();
            if (RestoringTime == null) throw new RestoringTimeNotSpecifiedException();
            if (ToDepart == null) throw new DepartConditionNotSpecifiedException();
            return H_Server.Start(load);
        }
        public Event<TScenario, TStatus> Depart() { return H_Server.Depart(); }
        #endregion

        #region Internal Events
        private class RestoreEvent : Event<TScenario, TStatus>
        {
            public RestoreServer<TScenario, TStatus, TLoad> RestoreServer { get; private set; }
            public TLoad Load { get; private set; }
            internal RestoreEvent(RestoreServer<TScenario, TStatus, TLoad> restoreServer, TLoad load)
            {
                RestoreServer = restoreServer;
                Load = load;
            }
            public override void Invoke()
            {
                Load.Log(this);
                foreach (var evnt in RestoreServer.OnRestore) Execute(evnt());
            }
            public override string ToString() { return string.Format("{0}_Restore", RestoreServer); }
        }
        #endregion

        #region Output Events - Reference to Event Generators
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnDepart { get { return H_Server.OnDepart; } }
        public List<Func<Event<TScenario, TStatus>>> OnRestore { get; private set; }
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
        public class DepartConditionNotSpecifiedException : Exception
        {
            public DepartConditionNotSpecifiedException() : base("Set ToDepart as depart condition.") { }
        }
        #endregion

        private static int _count = 0;
        public int Id { get; private set; }
        public string Tag { get; set; }
        public RestoreServer(int capacity = int.MaxValue, int seed = -1)
        {
            Id = _count++;
            Capacity = capacity;

            int hSeed = -1, rSeed = -1;
            if (seed < -1)
            {
                var rs = seed < 0 ? null : new Random(seed);
                hSeed = rs.Next();
                rSeed = rs.Next();
            }
            H_Server = new Server<TScenario, TStatus, TLoad>(seed: hSeed);
            R_Server = new Server<TScenario, TStatus, TLoad>(seed: rSeed);

            H_Server.OnDepart.Add(R_Server.Start);
            R_Server.ToDepart = () => true;
            R_Server.OnDepart.Add(l => new RestoreEvent(this, l));

            OnRestore = new List<Func<Event<TScenario, TStatus>>>();
        }
        public void WarmedUp(DateTime clockTime)
        {
            H_Server.WarmedUp(clockTime);
            R_Server.WarmedUp(clockTime);
        }
        public override string ToString()
        {
            if (Tag != null && Tag.Length > 0) return Tag;
            return string.Format("RestoreServer#{0}", Id);
        }
        public virtual void WriteToConsole()
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
