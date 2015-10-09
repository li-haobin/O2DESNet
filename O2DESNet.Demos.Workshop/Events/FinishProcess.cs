using System;

namespace O2DESNet.Demos.Workshop
{
    internal class FinishProcess : Event
    {
        internal Job Job { get; private set; }
        internal FinishProcess(Simulator sim, Job job) : base(sim) { Job = job; }
        public override void Invoke()
        {
            Console.WriteLine("{0}: Job #{1} finishes process.", _sim.ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), Job.Id);
            var waitingJob = _sim.Status.GetWaitingJob_onFinishProcess(Job);
            if (waitingJob != null) new StartProcess(_sim, waitingJob).Invoke();

            if (Job.CurrentMachineTypeIndex > 0) new StartProcess(_sim, Job).Invoke();
            else _sim.Status.Depart(Job);
        }
    }
}
