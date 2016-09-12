using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace Test
{
    public class GGnQueue<TScenario, TStatus, TLoad>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Static Properties
        public Func<Random, TimeSpan> InterArrivalTime
        {
            get { return Generator.InterArrivalTime; }
            set { Generator.InterArrivalTime = value; }
        }
        public Func<Random, TimeSpan> ServiceTime {
            get { return Server.ServiceTime; }
            set { Server.ServiceTime = value; }
        }
        public int ServerCapacity {
            get { return Server.Capacity; }
            set { Server.Capacity = value; }
        }
        public Func<TLoad> Create
        {
            get { return Generator.Create; }
            set { Generator.Create = value; }
        }
        #endregion

        #region Dynamic Properties
        public Generator<TScenario, TStatus, TLoad> Generator { get; private set; }
        public Queue<TScenario, TStatus, TLoad> Queue { get; private set; }
        public Server<TScenario, TStatus, TLoad> Server { get; private set; }

        internal Random RS { get; private set; }
        #endregion

        #region Input Events - Generators
        public Event<TScenario, TStatus> Start() { return Generator.Start(); }
        public Event<TScenario, TStatus> End() { return Generator.End(); }
        #endregion

        #region Output Events - Reference to Event Generators
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnDepart { get { return Server.OnDepart; } }
        #endregion

        private static int _count = 0;
        public int Id { get; protected set; }
        public GGnQueue(Func<Random, TimeSpan> interArrivalTime, Func<TLoad> create, Func<Random, TimeSpan> serviceTime, int serverCapacity, int seed = -1)
        {
            Id = _count++;     
            RS = seed < 0 ? null : new Random(seed);

            Server = new Server<TScenario, TStatus, TLoad>(seed: RS.Next())
            {
                ToDepart = () => true,
            };
            Queue = new Queue<TScenario, TStatus, TLoad>
            {
                ToDequeue = () => Server.Vancancy > 0,
            };
            Generator = new Generator<TScenario, TStatus, TLoad>(seed: RS.Next())
            {
                SkipFirst = false,
            };

            InterArrivalTime = interArrivalTime;
            Create = create;
            ServiceTime = serviceTime;
            ServerCapacity = serverCapacity;

            Generator.OnArrive.Add(Queue.Enqueue);
            Queue.OnDequeue.Add(Server.Start);
            Server.OnDepart.Add(load => Queue.Dequeue());
        }
        public void WarmedUp(DateTime clockTime)
        {
            Generator.WarmedUp(clockTime);
            Queue.WarmedUp(clockTime);
            Server.WarmedUp(clockTime);
        }
        public override string ToString() { return string.Format("GGnQueue#{0}", Id); }

        public virtual void WriteToConsole()
        {
            Console.WriteLine("===[{0}]===", this); Console.WriteLine();
            Queue.WriteToConsole(); Console.WriteLine();
            Server.WriteToConsole(); Console.WriteLine();
        }
    }
}
