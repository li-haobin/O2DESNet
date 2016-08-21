using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace Test
{
    /// <summary>
    /// The simulator class
    /// </summary>
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            Status.Generator.Create = () => new Load();
            Status.Generator.InterArrivalTime = Scenario.GetInterArrivalTime;
            Status.Generator.OnCreate = c => { Execute(Status.Queue.EnqueueEvent(c)); };

            Status.Queue.ToDequeue = Status.Server.HasVacancy;
            Status.Queue.OnDequeue = c => { Execute(Status.Server.StartEvent(c)); };

            Status.Server.ServiceTime = Scenario.GetServiceTime;
            Status.Server.OnFinish = c =>
            {
                Status.Queue.AttemptDequeue(ClockTime);
                Status.Processed.Add(c);
            };

            Execute(Status.Generator.ArriveEvent());
        }
    }
}
