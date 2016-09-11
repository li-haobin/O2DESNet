using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Generator<TScenario, TStatus, TLoad>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Static Properties
        public Func<Random, TimeSpan> InterArrivalTime { get; set; }
        public bool SkipFirst { get; set; } = true;
        public Func<TLoad> Create { get; set; }
        #endregion

        #region Dynamic Properties
        public DateTime? StartTime { get; private set; }
        public bool On { get; private set; }
        public int Count { get; private set; } // number of loads generated
        internal Random RS { get; private set; } // random stream
        #endregion

        #region Input Events - Generators
        public Event<TScenario, TStatus> Start() { return new StartEvent(this); }
        public Event<TScenario, TStatus> End() { return new EndEvent(this); }
        private class StartEvent : Event<TScenario, TStatus>
        {
            public Generator<TScenario, TStatus, TLoad> Generator { get; private set; }
            internal StartEvent(Generator<TScenario, TStatus, TLoad> generator) { Generator = generator; }
            public override void Invoke()
            {
                if (Generator.InterArrivalTime == null) throw new InterArrivalTimeNotSpecifiedException();
                Generator.On = true;
                Generator.StartTime = ClockTime;
                Generator.Count = 0;
                Execute(new ArriveEvent(Generator));
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
                    var load = Generator.Create();
                    load.Log(this);
                    Generator.Count++;                    
                    Schedule(new ArriveEvent(Generator), Generator.InterArrivalTime(Generator.RS == null ? DefaultRS : Generator.RS));
                    if (Generator.Count > 1 || !Generator.SkipFirst) foreach (var evnt in Generator.OnArrive) Execute(evnt(load));
                }
            }
            public override string ToString() { return string.Format("{0}_Arrive", Generator); }
        }
        #endregion

        #region Output Events - Reference to Event Generators
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnArrive { get; set; }
        #endregion

        #region Exeptions
        public class InterArrivalTimeNotSpecifiedException : Exception
        {
            public InterArrivalTimeNotSpecifiedException() : base("Set InterArrivalTime as a random generator.") { }
        }
        #endregion

        private static int _count = 0;
        public int Id { get; protected set; }
        public Generator(int seed = -1) {
            Id = _count++;
            On = false;
            RS = seed < 0 ? null : new Random(seed);
            Count = 0;
            OnArrive = new List<Func<TLoad, Event<TScenario, TStatus>>>();
        }
        public void WarmedUp(DateTime clockTime) { StartTime = clockTime; Count = 0; }
        public override string ToString() { return string.Format("Generator#{0}", Id); }
    }   
}
