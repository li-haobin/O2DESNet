using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet.Demos.GGnQueue
{
    public class GGnQueueSystem<TScenario, TStatus, TLoad> : Component
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Sub-Components
        public Generator<TScenario, TStatus, TLoad> Generator { get; private set; }
        public Queue<TScenario, TStatus, TLoad> Queue { get; private set; }
        public Server<TScenario, TStatus, TLoad> Server { get; private set; }
        #endregion

        #region Statics
        public class StaticProperties : O2DESNet.Scenario
        {
            public Func<Random, TimeSpan> InterArrivalTime { get; set; }
            public Func<TLoad, Random, TimeSpan> ServiceTime { get; set; }
            public int ServerCapacity { get; set; }
            public Func<TLoad> Create { get; set; }
        }
        public StaticProperties Statics { get; private set; }
        #endregion

        #region Dynamics
        public List<TLoad> Processed { get; private set; }
        public int NCompleted { get { return (int)Server.HourCounter.TotalDecrementCount; } }
        #endregion

        #region Events
        public class ArichiveEvent : Event<TScenario, TStatus>
        {
            public GGnQueueSystem<TScenario, TStatus, TLoad> GGnQueueSystem { get; private set; }
            public TLoad Load { get; private set; }
            public ArichiveEvent(GGnQueueSystem<TScenario, TStatus, TLoad> ggnQueueSystem, TLoad load) { GGnQueueSystem = ggnQueueSystem; Load = load; }
            public override void Invoke()
            {
                GGnQueueSystem.Processed.Add(Load);
            }
        }
        #endregion

        #region Input Events - Getters
        public Event<TScenario, TStatus> Start() { return Generator.Start(); }
        public Event<TScenario, TStatus> End() { return Generator.End(); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnDepart { get { return Server.OnDepart; } }
        #endregion

        #region Exeptions
        #endregion

        public GGnQueueSystem(StaticProperties statics, int seed, string tag = null) : base(seed, tag)
        {
            Name = "GGnQueueSystem";
            Statics = statics;
            Processed = new List<TLoad>();

            Generator = new Generator<TScenario, TStatus, TLoad>(
                statics: new Generator<TScenario, TStatus, TLoad>.StaticProperties
                {
                    Create = Statics.Create,
                    InterArrivalTime = Statics.InterArrivalTime,
                    SkipFirst = false,
                },
                seed: DefaultRS.Next());Queue = new Queue<TScenario, TStatus, TLoad>(
                statics: new Queue<TScenario, TStatus, TLoad>.StaticProperties
                {
                    ToDequeue = () => Server.Vancancy > 0,
                },
                tag: "Queue");
            Server = new Server<TScenario, TStatus, TLoad>(
               statics: new Server<TScenario, TStatus, TLoad>.StaticProperties
               {
                   Capacity = Statics.ServerCapacity,
                   ServiceTime = Statics.ServiceTime,
                   ToDepart = () => true,
               },
               seed: DefaultRS.Next(),
               tag: "2nd Server");

            Generator.OnArrive.Add(Queue.Enqueue);
            Queue.OnDequeue.Add(Server.Start);
            Server.OnDepart.Add(load => Queue.Dequeue());
            Server.OnDepart.Add(load => new ArichiveEvent(this, load));
        }

        public override void WarmedUp(DateTime clockTime)
        {
            Generator.WarmedUp(clockTime);
            Queue.WarmedUp(clockTime);
            Server.WarmedUp(clockTime);
        }

        public override void WriteToConsole()
        {
            Console.WriteLine("===[{0}]===", this); Console.WriteLine();
            Queue.WriteToConsole(); Console.WriteLine();
            Server.WriteToConsole(); Console.WriteLine();
            Console.WriteLine("Competed: {0}", NCompleted);
        }
    }
}
