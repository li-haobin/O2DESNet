using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Arrive<TScenario, TStatus, TLoad> : Event<TScenario, TStatus>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load
    {
        public Func<TLoad> Create { get; set; }
        public Action<TLoad> OnCreate { get; set; }
        public Func<Random, TimeSpan> InterArrivalTime { get; set; }

        public override void Invoke()
        {
            var load = Create();
            load.TimeStamp_Arrive = ClockTime;
            Log("{0} {1} arrives.", ClockTime.ToLongTimeString(), load);
            OnCreate(load);
            Schedule(new Arrive<TScenario, TStatus, TLoad>
            {
                Create = Create,
                OnCreate = OnCreate,
                InterArrivalTime = InterArrivalTime
            }, InterArrivalTime(DefaultRS));
        }
    }
}
