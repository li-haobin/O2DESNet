using System;

namespace O2DESNet.Demos.Workshop
{
    internal class StartProcess : Event
    {
        internal Job Job { get; private set; }
        internal StartProcess(Simulator sim, Job job) : base(sim) { Job = job; }
        public override void Invoke()
        {
            var machine = _sim.Status.Get_IdleMachine(Job.CurrentMachineTypeIndex);
            if (machine != null)
            {
                _sim.Status.Update_StartProcess(Job, machine);
                _sim.ScheduleEvent(
                    new FinishProcess(_sim, Job),
                    _sim.Scenario.Generate_ProcessingTime(Job.Type.Id, Job.CurrentMachineTypeIndex, _sim.RS));
            }
            else { _sim.Status.Enqueue(Job); }

            
        }
    }
}
