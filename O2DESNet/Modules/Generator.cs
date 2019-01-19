using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Generator<TLoad> : State<Generator<TLoad>.Statics>
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
        private abstract class InternalEvent : Event<Generator<TLoad>, Statics> { }
        private class StartEvent : InternalEvent
        {
            public override void Invoke()
            {
                if (!This.On)
                {
                    Log("Start");
                    if (Config.InterArrivalTime == null) throw new InterArrivalTimeNotSpecifiedException();
                    This.On = true;
                    This.StartTime = ClockTime;
                    This.Count = 0;
                    if (Config.SkipFirst) Schedule(new ArriveEvent(), Config.InterArrivalTime(DefaultRS));
                    else Schedule(new ArriveEvent());
                }
            }
        }
        private class EndEvent : InternalEvent
        {
            public override void Invoke()
            {
                if (This.On)
                {
                    Log("End");
                    This.On = false;
                }
            }
        }
        private class ArriveEvent : InternalEvent
        {
            public override void Invoke()
            {
                if (This.On)
                {
                    Log("Arrive");
                    var load = Config.Create(DefaultRS);
                    This.Count++;
                    Schedule(new ArriveEvent(), Config.InterArrivalTime(DefaultRS));
                    Execute(This.OnArrive.Select(e => e(load)));
                }
            }
            public override string ToString() { return string.Format("{0}_Arrive", This); }
        }
        #endregion

        #region Input Events - Getters
        public Event Start() { return new StartEvent { This = this }; }
        public Event End() { return new EndEvent { This = this }; }
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
