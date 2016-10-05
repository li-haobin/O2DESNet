using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Generator<TScenario, TStatus, TLoad> : Component
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Statics
        public class StaticProperties : Scenario
        {
            public Func<Random, TimeSpan> InterArrivalTime { get; set; }
            public bool SkipFirst { get; set; } = true;
            public Func<Random, TLoad> Create { get; set; }
        }
        public StaticProperties Statics { get; private set; }
        #endregion

        #region Dynamics
        public DateTime? StartTime { get; private set; }
        public bool On { get; private set; }
        public int Count { get; private set; } // number of loads generated   
        #endregion

        #region Events
        private class StartEvent : Event<TScenario, TStatus>
        {
            public Generator<TScenario, TStatus, TLoad> Generator { get; private set; }
            internal StartEvent(Generator<TScenario, TStatus, TLoad> generator) { Generator = generator; }
            public override void Invoke()
            {
                if (Generator.Statics.InterArrivalTime == null) throw new InterArrivalTimeNotSpecifiedException();
                Generator.On = true;
                Generator.StartTime = ClockTime;
                Generator.Count = 0;
                if (Generator.Statics.SkipFirst) Schedule(new ArriveEvent(Generator), Generator.Statics.InterArrivalTime(Generator.DefaultRS));
                else Execute(new ArriveEvent(Generator));
            }
        }
        private class EndEvent : Event<TScenario, TStatus>
        {
            public Generator<TScenario, TStatus, TLoad> Generator { get; private set; }
            internal EndEvent(Generator<TScenario, TStatus, TLoad> generator) { Generator = generator; }
            public override void Invoke() { Generator.On = false; }
        }
        private class ArriveEvent : Event<TScenario, TStatus>
        {
            public Generator<TScenario, TStatus, TLoad> Generator { get; private set; }
            internal ArriveEvent(Generator<TScenario, TStatus, TLoad> generator) { Generator = generator; }
            public override void Invoke()
            {
                if (Generator.On)
                {
                    var load = Generator.Statics.Create(Generator.DefaultRS);
                    load.Log(this);
                    Generator.Count++;
                    Schedule(new ArriveEvent(Generator), Generator.Statics.InterArrivalTime(Generator.DefaultRS));
                    foreach (var evnt in Generator.OnArrive) Execute(evnt(load));
                }
            }
            public override string ToString() { return string.Format("{0}_Arrive", Generator); }
        }
        #endregion

        #region Input Events - Getters
        public Event<TScenario, TStatus> Start() { return new StartEvent(this); }
        public Event<TScenario, TStatus> End() { return new EndEvent(this); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnArrive { get; private set; }
        #endregion

        #region Exeptions
        public class InterArrivalTimeNotSpecifiedException : Exception
        {
            public InterArrivalTimeNotSpecifiedException() : base("Set InterArrivalTime as a random generator.") { }
        }
        #endregion
        
        public Generator(StaticProperties statics, int seed, string tag = null) : base(seed, tag)
        {
            Name = "Generator";
            Statics = statics;
            On = false;
            Count = 0;

            // initialize for output events
            OnArrive = new List<Func<TLoad, Event<TScenario, TStatus>>>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            StartTime = clockTime;
            Count = 0;
        }        
    }   
}
