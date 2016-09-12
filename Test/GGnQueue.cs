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
            get { return Server2.ServiceTime; }
            set
            {
                Server1.ServiceTime = value;
                Server2.ServiceTime = value;
            }
        }
        public int ServerCapacity {
            get { return Server2.Capacity; }
            set
            {
                Server1.Capacity = value;
                Server2.Capacity = value;
            }
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
        public Server<TScenario, TStatus, TLoad> Server1 { get; private set; }
        public Server<TScenario, TStatus, TLoad> Server2 { get; private set; }
        public int NCompleted { get { return (int)Server2.HourCounter.TotalDecrementCount; } }

        internal Random RS { get; private set; }
        #endregion

        #region Input Events - Generators
        public Event<TScenario, TStatus> Start() { return Generator.Start(); }
        public Event<TScenario, TStatus> End() { return Generator.End(); }
        #endregion

        #region Output Events - Reference to Event Generators
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnDepart { get { return Server2.OnDepart; } }
        #endregion

        private static int _count = 0;
        public int Id { get; protected set; }
        public GGnQueue(Func<Random, TimeSpan> interArrivalTime, Func<TLoad> create, Func<Random, TimeSpan> serviceTime, int serverCapacity, int seed = -1)
        {
            Id = _count++;     
            RS = seed < 0 ? null : new Random(seed);

            Server2 = new Server<TScenario, TStatus, TLoad>(seed: RS.Next())
            {
                Tag = "2nd Server",
                ToDepart = () => true,
            };
            Server1 = new Server<TScenario, TStatus, TLoad>(seed: RS.Next())
            {
                Tag = "1st Server",
                ToDepart = () => Server2.Vancancy > 0,
            };
            Queue = new Queue<TScenario, TStatus, TLoad>
            {
                ToDequeue = () => Server1.Vancancy > 0,
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
            Queue.OnDequeue.Add(Server1.Start);
            Server1.OnDepart.Add(load => Queue.Dequeue());
            Server1.OnDepart.Add(Server2.Start);
            Server2.OnDepart.Add(load => Server1.Depart());
        }
        public void WarmedUp(DateTime clockTime)
        {
            Generator.WarmedUp(clockTime);
            Queue.WarmedUp(clockTime);
            Server2.WarmedUp(clockTime);
        }
        public override string ToString() { return string.Format("GGnQueue#{0}", Id); }

        public virtual void WriteToConsole()
        {
            Console.WriteLine("===[{0}]===", this); Console.WriteLine();
            Queue.WriteToConsole(); Console.WriteLine();
            Server1.WriteToConsole(); Console.WriteLine();
            Server2.WriteToConsole(); Console.WriteLine();
            Console.WriteLine("Competed: {0}", NCompleted);
        }
    }

    public class Load : Load<Scenario, Status> { }
}
