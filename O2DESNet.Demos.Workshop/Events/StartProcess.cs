using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                _sim.ScheduleEvent(new FinishProcess(_sim, Job), _sim.Scenario.Generate_ProcessingTime(Job.Type.Id, Job.CurrentMachineTypeIndex, _sim.RS));
                Console.WriteLine("{0}: Job #{1} starts process on Machine #{2}.", _sim.ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), Job.Id, machine.Id);
            }
            else
            {
                _sim.Status.Enqueue(Job);
                Console.WriteLine("{0}: Job #{1} queues.", _sim.ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), Job.Id);
            }

            
        }
    }
}
