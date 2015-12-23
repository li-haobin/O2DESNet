using O2DESNet.Demos.Workshop.Dynamics;
using System;

namespace O2DESNet.Demos.Workshop.Events
{
    internal class FinishProcess : Event<Scenario, Status>
    {
        internal Job Job { get; set; }
        protected override void Invoke()
        {
            Console.WriteLine("{0}: Job #{1} finishes process.", ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), Job.Id);
            var waitingJob = Status.GetWaitingJob_onFinishProcess(Job);
            if (waitingJob != null) Execute(new StartProcess { Job = waitingJob });

            if (Job.CurrentMachineTypeIndex > 0) Execute(new StartProcess { Job = Job });
            else Status.LogDeparture(Job, ClockTime);
        }
    }
}
