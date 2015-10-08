using System;

namespace O2DESNet.Demos.Workshop
{
    public class Simulator : O2DES
    {
        internal Scenario Scenario { get; private set; }
        internal Status Status { get; private set; }
        internal Random RS { get; private set; }

        public Simulator(Scenario scenario, int seed)
        {
            Scenario = scenario;
            Status = new Status(this);
            RS = new Random(seed);
            // schedule the initial event
            ScheduleEvent(new Arrival(this), Scenario.Generate_InterArrivalTime(RS));
        }       
    }
}
