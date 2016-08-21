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
        where TLoad : Load
    {
        public Func<TLoad> Create { get; set; }
        public Action<TLoad> OnCreate { get; set; }
        public Func<Random, TimeSpan> InterArrivalTime { get; set; }

        public Generator() { }

        public Arrive ArriveEvent() { return new Arrive(this); }

        public class Arrive : Event<TScenario, TStatus>
        {
            public Generator<TScenario, TStatus, TLoad> Generator { get; private set; }
            internal Arrive(Generator<TScenario, TStatus, TLoad> generator) { Generator = generator; }

            public override void Invoke()
            {
                var load = Generator.Create();
                load.TimeStamp_Arrive = ClockTime;
                Log("{0} {1} arrives.", ClockTime.ToLongTimeString(), load);
                Generator.OnCreate(load);
                Schedule(new Arrive(Generator), Generator.InterArrivalTime(DefaultRS));
            }
        }
    }   
}
