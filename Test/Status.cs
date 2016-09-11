using O2DESNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    /// <summary>
    /// The Status class that provides a snapshot of simulated system in run-time
    /// </summary>
    public class Status : Status<Scenario>
    {
        public Generator<Scenario, Status, Load> Generator { get; private set; }
        public Queue<Scenario, Status, Load> Queue { get; private set; }
        public Server<Scenario, Status, Load> Server { get; private set; }

        public List<Load<Scenario, Status>> Processed { get; private set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {            
            Processed = new List<Load<Scenario, Status>>();
            Server = new Server<Scenario, Status, Load>(seed: DefaultRS.Next())
            {
                Capacity = Scenario.ServerCapacity,
                ServiceTime = Scenario.GetServiceTime,                
            };
            Queue = new Queue<Scenario, Status, Load>
            {
                ToDequeue = () => Server.Vancancy > 0,
            };
            Generator = new Generator<Scenario, Status, Load>(seed: DefaultRS.Next())
            {
                InterArrivalTime = Scenario.GetInterArrivalTime,
                SkipFirst = false,
                Create = () => new Load(),
            };

            Generator.OnArrive.Add(Queue.Enqueue);
            Queue.OnDequeue.Add(Server.Start);
            Server.OnFinish.Add(load => Queue.Dequeue());
            Server.OnFinish.Add(load => new Depart(load));
        }

        public override void WarmedUp(DateTime clockTime)
        {
            Queue.WarmedUp(clockTime);
            Server.WarmedUp(clockTime);
            Processed = new List<Load<Scenario, Status>>();
        }
    }
}
