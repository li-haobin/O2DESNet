using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

namespace QueueDemo.Events
{
    /// <summary>
    /// O2DESNet Event - MyEvent2
    /// </summary>
    public class Arrival : O2DESNet.Event<Scenario, Status>
    {
        public override void Invoke()
        {

            if(Scenario.server.IsIdle)
            {
                Schedule(new StartService(), TimeSpan.Zero);
            }
            else
            {
                Scenario.queue.Length++;
                Scenario.queue.TimeStamp.Add(Simulator.ClockTime);
            }

            Schedule(new Arrival(), TimeSpan.FromMinutes(Exponential.Sample(DefaultRS, 1)));
        }
    }
}
