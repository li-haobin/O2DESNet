using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Components
{
    public class Arrive<TScenario, TStatus, TLoad> : Event<TScenario, TStatus>
       where TScenario : Scenario
       where TStatus : Status<TScenario>
    {
        public Func<TLoad> Create { get; set; }
        public Action<TLoad> OnCreate { get; set; }
        public Func<Random, TimeSpan> InterArrivalTime { get; set; }

        protected internal override void Invoke()
        {
            var load = Create();
            Log("{0} {1} arrives.", ClockTime.ToLongTimeString(), load);
            OnCreate(load);
            Schedule(new Arrive<TScenario, TStatus, TLoad> {
                Create = Create,
                OnCreate = OnCreate,
                InterArrivalTime = InterArrivalTime
            }, InterArrivalTime(DefaultRS));            
        }
    }
}
