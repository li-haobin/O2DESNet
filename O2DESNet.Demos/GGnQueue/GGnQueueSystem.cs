using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet.Demos.GGnQueue
{
    public class GGnQueueSystem : Component<GGnQueueSystem.Statics>
    {
        #region Sub-Components
        public Generator<Load> Generator { get; private set; }
        public Queue<Load> Queue { get; private set; }
        public Server<Load> Server { get; private set; }
        #endregion

        #region Statics
        public class Statics : Scenario
        {
            internal Generator<Load>.Statics Generator { get; private set; } = new Generator<Load>.Statics();
            internal Queue<Load>.Statics Queue { get; private set; } = new Queue<Load>.Statics();
            internal Server<Load>.Statics Server { get; private set; } = new Server<Load>.Statics();
            
            public Func<Random, TimeSpan> InterArrivalTime { get { return Generator.InterArrivalTime; } set { Generator.InterArrivalTime = value; } }
            public Func<Load, Random, TimeSpan> ServiceTime { get { return Server.ServiceTime; } set { Server.ServiceTime = value; } }
            public int ServerCapacity { get { return Server.Capacity; } set { Server.Capacity = value; } }            
        }
        #endregion

        #region Dynamics
        public List<Load> Processed { get; private set; }
        public int NCompleted { get { return (int)Server.HourCounter.TotalDecrementCount; } }
        #endregion

        #region Events
        private class ArchiveEvent : Event
        {
            public GGnQueueSystem GGnQueueSystem { get; private set; }
            public Load Load { get; private set; }
            public ArchiveEvent(GGnQueueSystem ggnQueueSystem, Load load) { GGnQueueSystem = ggnQueueSystem; Load = load; }
            public override void Invoke()
            {
                GGnQueueSystem.Processed.Add(Load);
            }
        }
        #endregion

        #region Input Events - Getters
        public Event Start() { return Generator.Start(); }
        public Event End() { return Generator.End(); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Load, Event>> OnDepart { get { return Server.OnDepart; } }
        #endregion

        #region Exeptions
        #endregion

        public GGnQueueSystem(Statics config, int seed = 0, string tag = null) : base(config, seed, tag)
        {
            Name = "GGnQueueSystem";
            Processed = new List<Load>();

            Config.Generator.Create = rs => new Load();
            Generator = new Generator<Load>(
                config: Config.Generator,
                seed: DefaultRS.Next());
            Generator.OnArrive.Add(load => Queue.Enqueue(load));

            Queue = new Queue<Load>(
                config: Config.Queue,
                tag: "Queue");
            Queue.Config.ToDequeue = load => Server.Vancancy > 0;
            Queue.OnDequeue.Add(load => Server.Start(load));

            Server = new Server<Load>(
               config: Config.Server,
               seed: DefaultRS.Next(),
               tag: "Server");
            Server.Config.ToDepart = load => true;
            Server.OnDepart.Add(load => Queue.Dequeue());
            Server.OnDepart.Add(load => new ArchiveEvent(this, load));

            InitEvents.Add(Generator.Start());
        }

        public override void WarmedUp(DateTime clockTime)
        {
            Generator.WarmedUp(clockTime);
            Queue.WarmedUp(clockTime);
            Server.WarmedUp(clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime = null)
        {
            Console.WriteLine("===[{0}]===", this); Console.WriteLine();
            Queue.WriteToConsole(); Console.WriteLine();
            Server.WriteToConsole(); Console.WriteLine();
            Console.WriteLine("Competed: {0}", NCompleted);
        }
    }
}
