using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Examples.MM1Queue
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            Execute(new Arrive<Scenario, Status, Load>
            {
                Create = () => new Load(),
                InterArrivalTime = Scenario.GetInterArrivalTime,
                OnCreate = c1 =>
                {
                    Execute(new Enqueue<Scenario, Status, Load>
                    {
                        Queue = Status.Queue,
                        Load = c1,
                        ToDequeue = Status.Processor.HasVacancy,
                        OnDequeue = c2 =>
                        {
                            Execute(new Start<Scenario, Status, Load>
                            {
                                Server = Status.Processor,
                                Load = c2,
                                ServiceTime = Scenario.GetServiceTime,
                                OnFinish = c3 =>
                                {
                                    Status.Queue.AttemptDequeue(ClockTime);
                                    Status.Processed.Add(c3);
                                },
                            });
                            Status.Queue.AttemptDequeue(ClockTime);
                        },
                    });
                },
            });
        }
    }
}
