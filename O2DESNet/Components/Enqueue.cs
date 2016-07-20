﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Enqueue<TScenario, TStatus, TLoad> : Event<TScenario, TStatus>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load
    {
        public Queue<TLoad> Queue { get; set; }
        public TLoad Load { get; set; }
        /// <summary>
        /// The condition to dequeue
        /// </summary>
        public Func<bool> ToDequeue { get; set; }
        /// <summary>
        /// The action to be exceuted on dequeuing
        /// </summary>
        public Action<TLoad> OnDequeue { get; set; }

        public override void Invoke()
        {
            Queue.Enqueue(Load, ToDequeue, OnDequeue, ClockTime);
            Queue.AttemptDequeue(ClockTime);
        }
    }
}