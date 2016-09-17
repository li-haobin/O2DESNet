using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet.Demos.TwoRestoreServer
{
    public class TwoRestoreServerSystem<TScenario, TStatus, TLoad> : Component
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Sub-Components
        public Generator<TScenario, TStatus, TLoad> Generator { get; private set; }
        public Queue<TScenario, TStatus, TLoad> Queue { get; private set; }
        public RestoreServer<TScenario, TStatus, TLoad> Server1 { get; private set; }
        public Queue<TScenario, TStatus, TLoad> Buffer { get; private set; }
        public RestoreServer<TScenario, TStatus, TLoad> Server2 { get; private set; }
        #endregion

        #region Statics
        public class StaticProperties : O2DESNet.Scenario
        {
            internal Generator<TScenario, TStatus, TLoad>.StaticProperties Generator { get; private set; } 
                = new Generator<TScenario, TStatus, TLoad>.StaticProperties();
            internal Queue<TScenario, TStatus, TLoad>.StaticProperties Queue { get; private set; } 
                = new Queue<TScenario, TStatus, TLoad>.StaticProperties();
            internal RestoreServer<TScenario, TStatus, TLoad>.StaticProperties Server1 { get; private set; } 
                = new RestoreServer<TScenario, TStatus, TLoad>.StaticProperties();
            internal Queue<TScenario, TStatus, TLoad>.StaticProperties Buffer { get; private set; }
                = new Queue<TScenario, TStatus, TLoad>.StaticProperties();
            internal RestoreServer<TScenario, TStatus, TLoad>.StaticProperties Server2 { get; private set; } 
                = new RestoreServer<TScenario, TStatus, TLoad>.StaticProperties();

            public Func<TLoad> Create { get { return Generator.Create; } set { Generator.Create = value; } }
            public Func<Random, TimeSpan> InterArrivalTime { get { return Generator.InterArrivalTime; } set { Generator.InterArrivalTime = value; } }
            public int ServerCapacity1 { get { return Server1.Capacity; } set { Server1.Capacity = value; } }
            public Func<TLoad, Random, TimeSpan> HandlingTime1 { get { return Server1.HandlingTime; } set { Server1.HandlingTime = value; } }
            public Func<TLoad, Random, TimeSpan> RestoringTime1 { get { return Server1.RestoringTime; } set { Server1.RestoringTime = value; } }
            public int BufferSize { get { return Buffer.Capacity; } set { Buffer.Capacity = value; } }
            public int ServerCapacity2 { get { return Server2.Capacity; } set { Server2.Capacity = value; } }
            public Func<TLoad, Random, TimeSpan> HandlingTime2 { get { return Server2.HandlingTime; } set { Server2.HandlingTime = value; } }
            public Func<TLoad, Random, TimeSpan> RestoringTime2 { get { return Server2.RestoringTime; } set { Server2.RestoringTime = value; } }
            public Func<TLoad, bool> ToDepart { get { return Server2.ToDepart; } set { Server2.ToDepart = value; } }            
        }
        public StaticProperties Statics { get; private set; }
        #endregion

        #region Dynamics
        public List<TLoad> Waiting { get { return Queue.Waiting; } }
        public HashSet<TLoad> Serving1 { get { return Server1.Serving; } }
        public List<TLoad> Served1 { get { return Server1.Served; } }
        public List<TLoad> InBuffer { get { return Buffer.Waiting; } }
        public HashSet<TLoad> Serving2 { get { return Server2.Serving; } }
        public List<TLoad> Served2 { get { return Server2.Served; } }        
        public int NCompleted { get { return Server2.NCompleted; } }
        public List<TLoad> Processed { get; private set; }
        #endregion

        #region Events
        private class ArchiveEvent : Event<TScenario, TStatus>
        {
            public TwoRestoreServerSystem<TScenario, TStatus, TLoad> System { get; private set; }
            public TLoad Load { get; private set; }
            public ArchiveEvent(TwoRestoreServerSystem<TScenario, TStatus, TLoad> system, TLoad load)
            {
                System = system;
                Load = load;
            }
            public override void Invoke()
            {
                System.Processed.Add(Load);
            }
        }
        #endregion

        #region Input Events - Getters
        public Event<TScenario, TStatus> Start() { return Generator.Start(); }
        public Event<TScenario, TStatus> End() { return Generator.End(); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnDepart { get { return Server2.OnDepart; } }
        #endregion

        #region Exeptions
        #endregion

        public TwoRestoreServerSystem(StaticProperties statics, int seed, string tag = null) : base(seed, tag)
        {
            Name = "TwoRestoreServerSystem";
            Statics = statics;
            Processed = new List<TLoad>();

            Generator = new Generator<TScenario, TStatus, TLoad>(
                statics: Statics.Generator,
                seed: DefaultRS.Next());
            Generator.OnArrive.Add(load => Queue.Enqueue(load));

            Queue = new Queue<TScenario, TStatus, TLoad>(
                 statics: Statics.Queue,
                 tag: "Queue");
            Queue.Statics.ToDequeue = load => Server1.Vancancy > 0;
            Queue.OnDequeue.Add(load => Server1.Start(load));

            Server1 = new RestoreServer<TScenario, TStatus, TLoad>(
               statics: Statics.Server1,
               seed: DefaultRS.Next(),
               tag: "1st Server");
            Server1.Statics.ToDepart = load => Buffer.Vancancy > 0;
            Server1.OnDepart.Add(load => Buffer.Enqueue(load));
            Server1.OnRestore.Add(() => Queue.Dequeue());

            Buffer = new Queue<TScenario, TStatus, TLoad>(
                 statics: Statics.Buffer,
                 tag: "Buffer");
            Buffer.Statics.ToDequeue = load => Server2.Vancancy > 0;
            Buffer.OnDequeue.Add(load => Server2.Start(load));
            Buffer.OnDequeue.Add(load => Server1.Depart());

            Server2 = new RestoreServer<TScenario, TStatus, TLoad>(
               statics: Statics.Server2,
               seed: DefaultRS.Next(),
               tag: "2st Server");
            Server2.Statics.ToDepart = load => true;
            Server2.OnDepart.Add(load => new ArchiveEvent(this, load));
            Server2.OnRestore.Add(() => Buffer.Dequeue());
        }

        public override void WarmedUp(DateTime clockTime)
        {
            Generator.WarmedUp(clockTime);
            Queue.WarmedUp(clockTime);
            Server1.WarmedUp(clockTime);
            Buffer.WarmedUp(clockTime);
            Server2.WarmedUp(clockTime);
        }

        public override void WriteToConsole()
        {
            Console.WriteLine("===[{0}]===", this); Console.WriteLine();
            Queue.WriteToConsole(); Console.WriteLine();
            Server1.WriteToConsole(); Console.WriteLine();
            Buffer.WriteToConsole(); Console.WriteLine();
            Server2.WriteToConsole(); Console.WriteLine();
            Console.WriteLine("Competed: {0}", NCompleted);
        }
    }
}
