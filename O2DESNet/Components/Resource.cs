using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet
{
    public class Resource<TLoad> : Component<Resource<TLoad>.Statics>
        where TLoad : Load
    {
        #region Sub-Components
        //internal Server<TScenario, TStatus, TLoad> H_Server { get; private set; }
        //internal Server<TScenario, TStatus, TLoad> R_Server { get; private set; }
        #endregion

        #region Statics
        public class Statics : Scenario
        {
            public Func<TLoad, double> Demand { get; set; }
            public double Capacity { get; set; }
        }
        #endregion

        #region Dynamics
        public HashSet<TLoad> Occupying { get; private set; }        
        public double Occupation { get { return Occupying.Sum(load => StaticProperty.Demand(load)); } }
        public double Vacancy { get { return StaticProperty.Capacity - Occupation; } }
        public bool HasVacancy(TLoad load) { return Vacancy >= StaticProperty.Demand(load); }
        public HourCounter HourCounter { get; private set; }
        #endregion

        #region Events
        private class OccupyEvent : Event
        {
            public Resource<TLoad> Resource { get; private set; }
            public TLoad Load { get; private set; }
            internal OccupyEvent(Resource<TLoad> resource, TLoad load)
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
        private class ReleaseEvent : Event
        {
            public Resource<TLoad> Resource { get; private set; }
            public TLoad Load { get; private set; }
            internal ReleaseEvent(Resource<TLoad> resource, TLoad load)
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
        public Event Occupy(TLoad load) { return new OccupyEvent(this, load); }
        public Event Release(TLoad load) { return new ReleaseEvent(this, load); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Event>> OnRelease { get; private set; }
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

        public Resource(Statics statics, int seed, string tag = null) : base(statics, seed, tag)
        {
            Name = "Resource";
            Occupying = new HashSet<TLoad>();
            HourCounter = new HourCounter();

            OnRelease = new List<Func<Event>>();
        }

        public override void WarmedUp(DateTime clockTime) { HourCounter.WarmedUp(clockTime); }

        public override void WriteToConsole()
        {
            Console.WriteLine("[{0}]", this);
            Console.Write("Occupation: {0}/{1}", Occupation, StaticProperty.Capacity);
            Console.Write("Occupying: ");
            foreach (var load in Occupying) Console.Write("{0} ", load);
            Console.WriteLine();
        }
    }
}
