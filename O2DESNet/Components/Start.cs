using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Start<TScenario, TStatus, TLoad> : Event<TScenario, TStatus>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load
    {
        public Processor<TLoad> Server { get; set; }
        public TLoad Load { get; set; }
        public Func<Random, TimeSpan> ServiceTime { get; set; }
        public Action<TLoad> OnFinish { get; set; }

        public override void Invoke()
        {
            Log("{0} {1} starts service.", ClockTime.ToLongTimeString(), Load);
            if (Load.TimeStamp_StartProcess == null) Load.TimeStamp_StartProcess = ClockTime;
            Server.Start(Load, ClockTime);
            Schedule(new Finish<TScenario, TStatus, TLoad> {
                Server = Server,
                Load = Load,
                OnFinish = OnFinish,
            }, ServiceTime(DefaultRS));            
        }
    }
}
