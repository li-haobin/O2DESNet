using System;

namespace O2DESNet.Demos.Workshop
{
    internal class Arrival : Event
    {
        internal Arrival(Simulator sim) : base(sim) { }
        public override void Invoke()
        {
            var job = _sim.Status.Generate_EnteringJob(_sim.RS);
            Console.WriteLine("{0}: Job #{1} (Type {2}) arrives.", _sim.ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), job.Id, job.Type.Id);
            new StartProcess(_sim, job).Invoke();
            // schedule the next arrival event
            _sim.ScheduleEvent(new Arrival(_sim), _sim.Scenario.Generate_InterArrivalTime(_sim.RS));
        }
    }
}
