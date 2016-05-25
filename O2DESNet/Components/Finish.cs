using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Components
{
    public class Finish<TScenario, TStatus, TLoad> : Event<TScenario, TStatus>
       where TScenario : Scenario
       where TStatus : Status<TScenario>
    {
        public Server<TLoad> Server { get; set; }
        public TLoad Load { get; set; }
        public Action<TLoad> OnFinish { get; set; }

        protected internal override void Invoke()
        {
            Log("{0} {1} finishes service.", ClockTime.ToLongTimeString(), Load);
            Server.Finish(Load, ClockTime);
            OnFinish(Load);            
        }
    }
}
