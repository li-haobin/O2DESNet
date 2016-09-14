using QueueDemo.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace QueueDemo
{
    /// <summary>
    /// The simulator class
    /// </summary>
    public class Simulator : O2DESNet.Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            // specify initial events here
            //throw new NotImplementedException();
            Schedule(new Arrival(), ClockTime);


            // example of scheduling a new event
            //Schedule(new Event(), TimeSpan.FromMinutes(DefaultRS.NextDouble() * 1));



            // example of executing a new event
            //Execute(new Arrival());
        }
    }
}
