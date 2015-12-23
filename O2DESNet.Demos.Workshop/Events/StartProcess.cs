using O2DESNet.Demos.Workshop.Dynamics;

namespace O2DESNet.Demos.Workshop.Events
{
    internal class StartProcess : Event<Scenario, Status>
    {
        internal Job Job { get; set; }
        protected override void Invoke()
        {
            var machine = Status.Get_IdleMachine(Job.CurrentMachineTypeIndex);
            if (machine != null)
            {
                Status.Update_StartProcess(Job, machine);
                Schedule(
                    new FinishProcess { Job = Job },
                    Scenario.Generate_ProcessingTime(Job.Type.Id, Job.CurrentMachineTypeIndex, DefaultRS));
            }
            else { Status.Enqueue(Job); }            
        }
    }
}
