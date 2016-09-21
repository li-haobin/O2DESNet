using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueDemo.Events
{
    /// <summary>
    /// O2DESNet Event - EndService
    /// </summary>
    public class EndService : O2DESNet.Event<Scenario, Status>
    {
        public override void Invoke()
        {
            Status.ServeCount++;

            if (Scenario.queue.Length>0)
            {

                Scenario.queue.Length--;
                var time = Scenario.queue.TimeStamp.First();

                Status.WaitingTime += Simulator.ClockTime - time;

                Scenario.queue.TimeStamp.RemoveAt(0);

                Schedule(new StartService(), TimeSpan.Zero);
            }
            else
            {
                Scenario.server.IsIdle = true;
            }
        }
    }
}
