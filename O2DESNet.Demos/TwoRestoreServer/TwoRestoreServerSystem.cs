using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet.Demos.TwoRestoreServer
{
    public class TwoRestoreServerSystem : Component<TwoRestoreServerSystem.Statics>
    {
        #region Sub-Components
        public Generator<Load> Generator { get; private set; }
        public Queuing<Load> Queue { get; private set; }
        public RestoreServer<Load> Server1 { get; private set; }
        public Queuing<Load> Buffer { get; private set; }
        public RestoreServer<Load> Server2 { get; private set; }
        #endregion

        #region Statics
        public class Statics : Scenario
        {
            internal Generator<Load>.Statics Generator { get; private set; } 
                = new Generator<Load>.Statics();
            internal Queuing<Load>.Statics Queue { get; private set; } 
                = new Queuing<Load>.Statics();
            internal RestoreServer<Load>.Statics Server1 { get; private set; } 
                = new RestoreServer<Load>.Statics();
            internal Queuing<Load>.Statics Buffer { get; private set; }
                = new Queuing<Load>.Statics();
            internal RestoreServer<Load>.Statics Server2 { get; private set; } 
                = new RestoreServer<Load>.Statics();
            
            public Func<Random, TimeSpan> InterArrivalTime { get { return Generator.InterArrivalTime; } set { Generator.InterArrivalTime = value; } }
            public int ServerCapacity1 { get { return Server1.Capacity; } set { Server1.Capacity = value; } }
            public Func<Load, Random, TimeSpan> HandlingTime1 { get { return Server1.HandlingTime; } set { Server1.HandlingTime = value; } }
            public Func<Load, Random, TimeSpan> RestoringTime1 { get { return Server1.RestoringTime; } set { Server1.RestoringTime = value; } }
            public int BufferSize { get { return Buffer.Capacity; } set { Buffer.Capacity = value; } }
            public int ServerCapacity2 { get { return Server2.Capacity; } set { Server2.Capacity = value; } }
            public Func<Load, Random, TimeSpan> HandlingTime2 { get { return Server2.HandlingTime; } set { Server2.HandlingTime = value; } }
            public Func<Load, Random, TimeSpan> RestoringTime2 { get { return Server2.RestoringTime; } set { Server2.RestoringTime = value; } }
            //public Func<Load, bool> ToDepart { get { return Server2.ToDepart; } set { Server2.ToDepart = value; } }            
        }
        #endregion

        #region Dynamics
        public List<Load> Waiting { get { return Queue.Waiting; } }
        public HashSet<Load> Serving1 { get { return Server1.Serving; } }
        public HashSet<Load> Served1 { get { return Server1.Served; } }
        public List<Load> InBuffer { get { return Buffer.Waiting; } }
        public HashSet<Load> Serving2 { get { return Server2.Serving; } }
        public HashSet<Load> Served2 { get { return Server2.Served; } }        
        public int NCompleted { get { return Server2.NCompleted; } }
        public List<Load> Processed { get; private set; }
        #endregion

        #region Events
        private class ArchiveEvent : Event
        {
            public TwoRestoreServerSystem System { get; private set; }
            public Load Load { get; private set; }
            public ArchiveEvent(TwoRestoreServerSystem system, Load load)
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
        public Event Start() { return Generator.Start(); }
        public Event End() { return Generator.End(); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Load, Event>> OnDepart { get { return Server2.OnDepart; } }
        #endregion

        #region Exeptions
        #endregion

        public TwoRestoreServerSystem(Statics config, int seed = 0, string tag = null) : base(config, seed, tag)
        {
            Name = "TwoRestoreServerSystem";
            Processed = new List<Load>();

            Config.Generator.Create = rs => new Load();
            Generator = new Generator<Load>(
                config: Config.Generator,
                seed: DefaultRS.Next());
            Generator.OnArrive.Add(load => Queue.Enqueue(load));

            Queue = new Queuing<Load>(
                 config: Config.Queue,
                 tag: "Queue");
            Queue.OnDequeue.Add(load => Server1.Start(load));

            Server1 = new RestoreServer<Load>(
               config: Config.Server1,
               seed: DefaultRS.Next(),
               tag: "1st Server");
            Server1.OnDepart.Add(load => Buffer.Enqueue(load));
            Server1.OnStateChg.Add(s => Queue.UpdToDequeue(s.Vacancy > 0));

            Buffer = new Queuing<Load>(
                 config: Config.Buffer,
                 tag: "Buffer");
            Buffer.OnDequeue.Add(load => Server2.Start(load));
            Buffer.OnStateChg.Add(b => Server1.UpdToDepart(b.Vacancy > 0));

            Server2 = new RestoreServer<Load>(
               config: Config.Server2,
               seed: DefaultRS.Next(),
               tag: "2st Server");
            Server2.OnDepart.Add(load => new ArchiveEvent(this, load));
            Server2.OnStateChg.Add(s => Buffer.UpdToDequeue(s.Vacancy > 0));

            InitEvents.Add(Generator.Start());
        }

        public override void WarmedUp(DateTime clockTime)
        {
            Generator.WarmedUp(clockTime);
            Queue.WarmedUp(clockTime);
            Server1.WarmedUp(clockTime);
            Buffer.WarmedUp(clockTime);
            Server2.WarmedUp(clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime = null)
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
