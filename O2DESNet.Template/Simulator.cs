using System;

namespace O2DESNet.Template
{
    public class Simulator : O2DES
    {
        internal Scenario Scenario { get; private set; }
        internal Status Status { get; private set; }
        internal Random RS { get; private set; } // use multiple random streams if necessary

        public Simulator(Scenario scenario, int seed)
        {
            Scenario = scenario;
            Status = new Status(this);
            RS = new Random(seed);

            // schedule the initial event
            // ScheduleEvent(new MyEvent1(this, new Load_1()), TimeSpan.FromHours(some_random_value));
        }
    }
}
