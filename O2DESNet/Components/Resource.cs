using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet
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
        public double Occupation { get { return Occupying.Sum(load => Statics.Demand(load)); } }
        public double Vacancy { get { return Statics.Capacity - Occupation; } }
        public bool HasVacancy(TLoad load) { return Vacancy >= Statics.Demand(load); }
        public HourCounter HourCounter { get; private set; }
        #endregion

        #region Events
        private class OccupyEvent : Event<TScenario, TStatus>
        {
            public Resource<TScenario, TStatus, TLoad> Resource { get; private set; }
            public TLoad Load { get; private set; }
            internal OccupyEvent(Resource<TScenario, TStatus, TLoad> resource, TLoad load)
            {
                Resource = resource;
                Load = load;
            }
            public override void Invoke()
            {
                if (!Resource.HasVacancy(Load)) throw new InsufficientVacancyException();
                Load.Log(this);
                Resource.Occupying.Add(Load);
                Resource.HourCounter.ObserveCount(Resource.Occupation, ClockTime);
            }
            public override string ToString() { return string.Format("{0}_Occupy", Resource); }
        }
        private class ReleaseEvent : Event<TScenario, TStatus>
        {
            public Resource<TScenario, TStatus, TLoad> Resource { get; private set; }
            public TLoad Load { get; private set; }
            internal ReleaseEvent(Resource<TScenario, TStatus, TLoad> resource, TLoad load)
            {
                Resource = resource;
                Load = load;
            }
            public override void Invoke()
            {
                if (!Resource.Occupying.Contains(Load)) throw new NotOccupyingException();
                Load.Log(this);
                Resource.Occupying.Remove(Load);
                Resource.HourCounter.ObserveCount(Resource.Occupation, ClockTime);
                foreach (var evnt in Resource.OnRelease) Execute(evnt());
            }
            public override string ToString() { return string.Format("{0}_Release", Resource); }
        }
        #endregion

        #region Input Events - Getters
        public Event<TScenario, TStatus> Occupy(TLoad load) { return new OccupyEvent(this, load); }
        public Event<TScenario, TStatus> Release(TLoad load) { return new ReleaseEvent(this, load); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Event<TScenario, TStatus>>> OnRelease { get; private set; }
        #endregion

        #region Exeptions
        public class InsufficientVacancyException : Exception
        {
            public InsufficientVacancyException() : base("Check vacancy of the Resource before execute Occupy event.") { }
        }
        public class NotOccupyingException : Exception
        {
            public NotOccupyingException() : base("The specified Load is not ocuppying the Resource.") { }
        }
        #endregion

        public Resource(StaticProperties statics, int seed, string tag = null) : base(seed, tag)
        {
            Name = "Resource";
            Statics = statics;
            Occupying = new HashSet<TLoad>();
            HourCounter = new HourCounter();

            OnRelease = new List<Func<Event<TScenario, TStatus>>>();
        }

        public override void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }

        public override void WriteToConsole()
        {
            Console.WriteLine("[{0}]", this);
            Console.Write("Occupation: {0}/{1}", Occupation, Statics.Capacity);
            Console.Write("Occupying: ");
            foreach (var load in Occupying) Console.Write("{0} ", load);
            Console.WriteLine();
        }
    }
}
