using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet.Components
{
    public class Synchronizer : Component<Synchronizer.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            //    public Func<TLoad, Random, TimeSpan> HandlingTime { get { return H_Server.ServiceTime; } set { H_Server.ServiceTime = value; } }
            //    public Func<TLoad, Random, TimeSpan> RestoringTime { get { return R_Server.ServiceTime; } set { R_Server.ServiceTime = value; } }
            //    public Func<bool> ToDepart { get { return H_Server.ToDepart; } set { H_Server.ToDepart = value; } }
            //    public int Capacity { get; set; }
        }
        #endregion

        #region Sub-Components
        //internal Server<TScenario, TStatus, TLoad> H_Server { get; private set; }
        //internal Server<TScenario, TStatus, TLoad> R_Server { get; private set; }
        #endregion

        #region Dynamics
        //public HashSet<TLoad> Serving { get { return H_Server.Serving; } }
        //public List<TLoad> Served { get { return H_Server.Served; } }
        //public HashSet<TLoad> Restoring { get { return R_Server.Serving; } }
        //public int Vancancy { get { return Statics.Capacity - Serving.Count - Served.Count - Restoring.Count; } }
        //public int NCompleted { get { return (int)H_Server.HourCounter.TotalDecrementCount; } }     
        #endregion

        #region Events
        //private class RestoreEvent : Event
        //{
        //    public RestoreServer<TLoad> RestoreServer { get; private set; }
        //    public TLoad Load { get; private set; }
        //    internal RestoreEvent(RestoreServer<TLoad> restoreServer, TLoad load)
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
        //public Event Depart() { return H_Server.Depart(); }
        //public Event Start(TLoad load)
        //{
        //    if (Vancancy < 1) throw new HasZeroVacancyException();
        //    if (Statics.HandlingTime == null) throw new HandlingTimeNotSpecifiedException();
        //    if (Statics.RestoringTime == null) throw new RestoringTimeNotSpecifiedException();
        //    if (Statics.ToDepart == null) throw new DepartConditionNotSpecifiedException();
        //    return H_Server.Start(load);
        //}
        //public Event Depart() { return new DepartEvent(this); }
        #endregion

        #region Output Events - Reference to Getters
        //public List<Func<TLoad, Event>> OnDepart { get { return H_Server.OnDepart; } }
        public List<Func<bool, Event>> OnReady { get; private set; }
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

        public Synchronizer(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Synchronizer";

            //H_Server = new Server<TLoad>(statics.H_Server, DefaultRS.Next());
            //R_Server = new Server<TLoad>(statics.R_Server, DefaultRS.Next());

            // connect sub-components
            //H_Server.OnDepart.Add(R_Server.Start());
            //R_Server.Statics.ToDepart = load => true;
            //R_Server.OnDepart.Add(l => new RestoreEvent(this, l));

            // initialize for output events
            //OnRestore = new List<Func<Event>>(); 

            // initialize event, compulsory if it's assembly
            //InitEvents.Add(R_Server.Start());
        }

        public override void WarmedUp(DateTime clockTime)
        {
            //H_Server.WarmedUp(clockTime);
            //R_Server.WarmedUp(clockTime);
        }
        
        public override void WriteToConsole(DateTime? clockTime = default(DateTime?))
        {
            throw new NotImplementedException();
        }
    }
}
