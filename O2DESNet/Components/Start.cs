﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Components
{
    public class Start<TScenario, TStatus, TLoad> : Event<TScenario, TStatus>
       where TScenario : Scenario
       where TStatus : Status<TScenario>
    {
        public Server<TLoad> Server { get; set; }
        public TLoad Load { get; set; }
        public Func<Random, TimeSpan> ServiceTime { get; set; }
        public Action<TLoad> OnFinish { get; set; }

        protected internal override void Invoke()
        {
            Log("{0} {1} starts service.", ClockTime.ToLongTimeString(), Load);
            Server.Start(Load, ClockTime);
            Schedule(new Finish<TScenario, TStatus, TLoad> {
                Server = Server,
                Load = Load,
                OnFinish = OnFinish,
            }, ServiceTime(DefaultRS));            
        }
    }
}
