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
            internal Generator<TScenario, TStatus, TLoad>.StaticProperties Generator { get; private set; } = new Generator<TScenario, TStatus, TLoad>.StaticProperties();
            internal Queue<TScenario, TStatus, TLoad>.StaticProperties Queue { get; private set; } = new Queue<TScenario, TStatus, TLoad>.StaticProperties();
            internal Server<TScenario, TStatus, TLoad>.StaticProperties Server { get; private set; } = new Server<TScenario, TStatus, TLoad>.StaticProperties();

            public Func<TLoad> Create { get { return Generator.Create; } set { Generator.Create = value; } }
            public Func<Random, TimeSpan> InterArrivalTime { get { return Generator.InterArrivalTime; } set { Generator.InterArrivalTime = value; } }
            public Func<TLoad, Random, TimeSpan> ServiceTime { get { return Server.ServiceTime; } set { Server.ServiceTime = value; } }
            public int ServerCapacity { get { return Server.Capacity; } set { Server.Capacity = value; } }            
        }
        public StaticProperties Statics { get; private set; }
        #endregion

        #region Dynamics
        public List<TLoad> Processed { get; private set; }
        public int NCompleted { get { return (int)Server.HourCounter.TotalDecrementCount; } }
        #endregion

        #region Events
        private class ArchiveEvent : Event<TScenario, TStatus>
        {


            public GGnQueueSystem<TScenario, TStatus, TLoad> GGnQueueSystem { get; private set; }
            public TLoad Load { get; private set; }
            public ArchiveEvent(GGnQueueSystem<TScenario, TStatus, TLoad> ggnQueueSystem, TLoad load) { GGnQueueSystem = ggnQueueSystem; Load = load; }
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
                statics: Statics.Generator,
                seed: DefaultRS.Next());
            Generator.OnArrive.Add(load => Queue.Enqueue(load));

            Queue = new Queue<TScenario, TStatus, TLoad>(
                statics: Statics.Queue,
                tag: "Queue");
            Queue.Statics.ToDequeue = load => Server.Vancancy > 0;
            Queue.OnDequeue.Add(load => Server.Start(load));

            Server = new Server<TScenario, TStatus, TLoad>(
               statics: Statics.Server,
               seed: DefaultRS.Next(),
               tag: "Server");
            Server.Statics.ToDepart = load => true;
            Server.OnDepart.Add(load => Queue.Dequeue());
            Server.OnDepart.Add(load => new ArchiveEvent(this, load));
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
