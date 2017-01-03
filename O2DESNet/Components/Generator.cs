using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Generator<TLoad> : Component<Generator<TLoad>.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public Func<Random, TimeSpan> InterArrivalTime { get; set; }
            public bool SkipFirst { get; set; } = true;
            public Func<Random, TLoad> Create { get; set; }
        }
        #endregion

        #region Dynamics
        public DateTime? StartTime { get; private set; }
        public bool On { get; private set; }
        public int Count { get; private set; } // number of loads generated   
        #endregion

        #region Events
        private class StartEvent : Event
        {
            public Generator<TLoad> Generator { get; private set; }
            internal StartEvent(Generator<TLoad> generator) { Generator = generator; }
            public override void Invoke()
            {
                if (Generator.Config.InterArrivalTime == null) throw new InterArrivalTimeNotSpecifiedException();
                Generator.On = true;
                Generator.StartTime = ClockTime;
                Generator.Count = 0;
                if (Generator.Config.SkipFirst) Schedule(new ArriveEvent(Generator), Generator.Config.InterArrivalTime(Generator.DefaultRS));
                else Execute(new ArriveEvent(Generator));
            }
        }
        private class EndEvent : Event
        {
            public Generator<TLoad> Generator { get; private set; }
            internal EndEvent(Generator<TLoad> generator) { Generator = generator; }
            public override void Invoke() { Generator.On = false; }
        }
        private class ArriveEvent : Event
        {
            public Generator<TLoad> Generator { get; private set; }
            internal ArriveEvent(Generator<TLoad> generator) { Generator = generator; }
            public override void Invoke()
            {
                if (Generator.On)
                {
                    var load = Generator.Config.Create(Generator.DefaultRS);
                    Generator.Count++;
                    Schedule(new ArriveEvent(Generator), Generator.Config.InterArrivalTime(Generator.DefaultRS));
                    foreach (var evnt in Generator.OnArrive) Execute(evnt(load));
                }
            }
            public override string ToString() { return string.Format("{0}_Arrive", Generator); }
        }
        #endregion

        #region Input Events - Getters
        public Event Start() { return new StartEvent(this); }
        public Event End() { return new EndEvent(this); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event>> OnArrive { get; private set; }
        #endregion

        #region Exeptions
        public class InterArrivalTimeNotSpecifiedException : Exception
        {
            public InterArrivalTimeNotSpecifiedException() : base("Set InterArrivalTime as a random generator.") { }
        }
        #endregion
        
        public Generator(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Generator";
            On = false;
            Count = 0;

            // initialize for output events
            OnArrive = new List<Func<TLoad, Event>>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            StartTime = clockTime;
            Count = 0;
        }

        public override void WriteToConsole(DateTime? clockTime = null)
        {
            throw new NotImplementedException();
        }
    }   
}
