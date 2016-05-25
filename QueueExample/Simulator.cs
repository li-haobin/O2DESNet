using O2DESNet.Components;
using QueueExample.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueExample
{
    public class Simulator : O2DESNet.Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            Execute(new Arrive<Scenario, Status, Customer>
            {
                Create = () => new Customer(),
                InterArrivalTime = rs => TimeSpan.FromMinutes(rs.NextDouble() * 10),
                OnCreate = c1 => {
                    Execute(new Enqueue<Scenario, Status, Customer>
                    {
                        Queue = Status.Queue,
                        Load = c1,
                        ToDequeue = () => Status.Server.IsIdle,
                        OnDequeue = c2 => {
                            Execute(new Start<Scenario, Status, Customer>
                            {
                                Server = Status.Server,
                                Load = c2,
                                ServiceTime = rs => TimeSpan.FromMinutes(rs.NextDouble() * 10),
                                OnFinish = c3 => { Status.Queue.AttemptDequeue(ClockTime); },
                            });
                            Status.Queue.AttemptDequeue(ClockTime);
                            },
                    });
                },                
            });
        }
    }
}
