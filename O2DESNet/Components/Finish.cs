using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Finish<TScenario, TStatus, TLoad> : Event<TScenario, TStatus>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load
    {
        public Processor<TLoad> Server { get; set; }
        public TLoad Load { get; set; }
        public Action<TLoad> OnFinish { get; set; }

        protected internal override void Invoke()
        {
            Log("{0} {1} finishes service.", ClockTime.ToLongTimeString(), Load);
            if (Load.TimeStamp_FinishProcess == null) Load.TimeStamp_FinishProcess = ClockTime;
            Server.Finish(Load, ClockTime);
            OnFinish(Load);
        }
    }
}
