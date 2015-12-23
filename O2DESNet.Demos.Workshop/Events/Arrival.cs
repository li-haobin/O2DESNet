using System;

namespace O2DESNet.Demos.Workshop.Events
{
    internal class Arrival : Event<Scenario, Status>
    {
        protected override void Invoke()
        {
            var job = Status.Generate_EnteringJob(DefaultRS, ClockTime);
            Console.WriteLine("{0}: Job #{1} (Type {2}) arrives.", ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), job.Id, job.Type.Id);
            Execute(new StartProcess { Job = job });
            // schedule the next arrival event
            Schedule(new Arrival(), Scenario.Generate_InterArrivalTime(DefaultRS));
        }
    }
}
