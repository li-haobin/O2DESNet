using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet.Components
{
    public class Resource<TScenario, TStatus, TLoad> : Component
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Sub-Components
        //internal Server<TScenario, TStatus, TLoad> H_Server { get; private set; }
        //internal Server<TScenario, TStatus, TLoad> R_Server { get; private set; }
        #endregion

        #region Statics
        public class StaticProperties : Scenario
        {
            public Func<TLoad, double> Demand { get; set; }
            public double Capacity { get; set; }
        }
        public StaticProperties Statics { get; private set; }
        #endregion

        #region Dynamics
        public HashSet<TLoad> Occupying { get; private set; }
        public double Vancancy { get { return Statics.Capacity - Occupying.Sum(load => Statics.Demand(load)); } }
        public bool HasVecancy(TLoad load) { return Vancancy >= Statics.Demand(load); }
        #endregion

        #region Events
        //private class RestoreEvent : Event<TScenario, TStatus>
        //{
        //    public RestoreServer<TScenario, TStatus, TLoad> RestoreServer { get; private set; }
        //    public TLoad Load { get; private set; }
        //    internal RestoreEvent(RestoreServer<TScenario, TStatus, TLoad> restoreServer, TLoad load)
        //    {
        //        RestoreServer = restoreServer;
        //        Load = load;
        //    }
        //    public override void Invoke()
        //    {
        //        Load.Log(this);
        //        foreach (var evnt in RestoreServer.OnRestore) Execute(evnt());
        //    }
        //    public override string ToString() { return string.Format("{0}_Restore", RestoreServer); }
        //}
        #endregion

        #region Input Events - Getters
        //public Event<TScenario, TStatus> Depart() { return H_Server.Depart(); }
        //public Event<TScenario, TStatus> Start(TLoad load)
        //{
        //    if (Vancancy < 1) throw new HasZeroVacancyException();
        //    if (Statics.HandlingTime == null) throw new HandlingTimeNotSpecifiedException();
        //    if (Statics.RestoringTime == null) throw new RestoringTimeNotSpecifiedException();
        //    if (Statics.ToDepart == null) throw new DepartConditionNotSpecifiedException();
        //    return H_Server.Start(load);
        //}
        //public Event<TScenario, TStatus> Depart() { return new DepartEvent(this); }
        #endregion

        #region Output Events - Reference to Getters
        //public List<Func<TLoad, Event<TScenario, TStatus>>> OnDepart { get { return H_Server.OnDepart; } }
        //public List<Func<Event<TScenario, TStatus>>> OnRestore { get; private set; }
        #endregion

        #region Exeptions
        //public class HasZeroVacancyException : Exception
        //{
        //    public HasZeroVacancyException() : base("Check vacancy of the Server before execute Start event.") { }
        //}
        //public class HandlingTimeNotSpecifiedException : Exception
        //{
        //    public HandlingTimeNotSpecifiedException() : base("Set HandlingTime as a random generator.") { }
        //}
        //public class RestoringTimeNotSpecifiedException : Exception
        //{
        //    public RestoringTimeNotSpecifiedException() : base("Set RestoringTime as a random generator.") { }
        //}
        //public class DepartConditionNotSpecifiedException : Exception
        //{
        //    public DepartConditionNotSpecifiedException() : base("Set ToDepart as depart condition.") { }
        //}
        #endregion

        public Resource(StaticProperties statics, int seed, string tag = null) : base(seed, tag)
        {
            Name = "Resource";
            Statics = statics;
            //H_Server = new Server<TScenario, TStatus, TLoad>(statics.H_Server, DefaultRS.Next());
            //R_Server = new Server<TScenario, TStatus, TLoad>(statics.R_Server, DefaultRS.Next());

            // connect sub-components
            //H_Server.OnDepart.Add(R_Server.Start);
            //R_Server.Statics.ToDepart = () => true;
            //R_Server.OnDepart.Add(l => new RestoreEvent(this, l));

            // initialize for output events
            //OnRestore = new List<Func<Event<TScenario, TStatus>>>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            //H_Server.WarmedUp(clockTime);
            //R_Server.WarmedUp(clockTime);
        }

        public override void WriteToConsole()
        {
            //Console.WriteLine("[{0}]", this);
            //Console.Write("Serving: ");
            //foreach (var load in Serving) Console.Write("{0} ", load);
            //Console.WriteLine();
            //Console.Write("Served: ");
            //foreach (var load in Served) Console.Write("{0} ", load);
            //Console.WriteLine();
            //Console.Write("Restoring: ");
            //foreach (var load in Restoring) Console.Write("{0} ", load);
            //Console.WriteLine();
        }
    }
}
