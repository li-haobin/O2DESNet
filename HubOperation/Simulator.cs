using HubOperation.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation
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


            // example of scheduling a new event
            //Schedule(new Event(), TimeSpan.FromMinutes(DefaultRS.NextDouble() * 1));
            Schedule(new AircraftArrival(), TimeSpan.FromHours(1));

            // example of executing a new event
            Execute(new StartSortOperation());
        }
    }
}
