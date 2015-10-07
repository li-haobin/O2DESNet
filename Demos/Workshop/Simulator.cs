using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.Workshop
{
    public class Simulator : O2DESNet.Simulator
    {
        internal Scenario Scenario { get; private set; }
        internal Status Status { get; private set; }

        public Simulator(Scenario scenario, int seed)
        {
            Scenario = scenario;
            Status = new Status(this, Scenario, seed);
            new Initialization(this).Invoke();
        }
        
        private O2DESNet.Event Arrival()
        {
            return delegate()
            {
                var job = Status.Generate_EnteringJob(RS);
                //Console.WriteLine("{0}: Job #{1} (Type {2}) Arrives.", ClockTime, job.Id, job.Type);
                Process(job)();
                ScheduleEvent(Arrival(), ClockTime + Scenario.Generate_InterArrivalTime(RS));
            };
        }

        private O2DESNet.Event Process(Job job)
        {
            return delegate()
            {
                var machine = Status.Get_IdleMachine(job.CurrentMachineTypeIndex);
                if (machine != null)
                {
                    Status.StartProcessing(job, machine);
                    ScheduleEvent(Finish(job), ClockTime + Scenario.Generate_ProcessingTime(job.Type.Id, job.CurrentMachineTypeIndex, RS));
                    //Console.WriteLine("{0}: Job #{1} (Type {2}) starts process at Machine (Type {3}).", ClockTime, job.Id, job.Type, machine.Type);
                }
                else
                {
                    Status.Enqueue(job);
                }
            };
        }

        private O2DESNet.Event Finish(Job job)
        {
            return delegate()
            {
                var waitingJob = Status.FinishProcessing(job);
                if (waitingJob != null) Process(waitingJob)();   
                             
                if (job.CurrentMachineTypeIndex > 0) Process(job)();
                else Status.Depart(job);                
            };
        }
        
    }
}
