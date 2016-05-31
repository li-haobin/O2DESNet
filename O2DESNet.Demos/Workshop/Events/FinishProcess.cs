using O2DESNet.Demos.Workshop.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Demos.Workshop.Events
{
    internal class FinishProcess : Event<Scenario, Status>
    {
        internal Machine Machine { get; set; }
        public override void Invoke()
        {
            var prodects = Machine.Processing;
            Machine.Processing = null;
            foreach (var p in prodects)
            {
                Log("{0}: Product #{1} @ Machine #{2} finishes process.", ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), p.Id, Machine.Id);
                p.CurrentStage++;
                p.BeingProcessedBy = null;
                if (p.CurrentWorkStation != null) Status.Queues[p.CurrentWorkStation].Add(p); // push product to next process
                else Execute(new Depart { Product = p });                
            }
            foreach (var ws in prodects.Select(p => p.CurrentWorkStation).Distinct())
                Execute(new AttemptToProcess { WorkStation = ws }); // attemp to process at each relevant work station            
            Execute(new AttemptToProcess { WorkStation = Machine.WorkStation }); // attemp to pull products at current work station            
        }
    }
}
