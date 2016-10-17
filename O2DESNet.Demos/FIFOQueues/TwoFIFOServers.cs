using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet.Demos.FIFOQueues
{
    public class TwoFIFOServers : Component<TwoFIFOServers.Statics>
    {
        #region Sub-Components
        public Generator<Load> Generator { get; private set; }
        public Queue<Load> Queue { get; private set; }
        public FIFOServer<Load> Server1 { get; private set; }
        public Queue<Load> Buffer { get; private set; }
        public FIFOServer<Load> Server2 { get; private set; }
        #endregion

        #region Statics
        public class Statics : Scenario
        {
            internal Generator<Load>.Statics Generator { get; private set; } 
                = new Generator<Load>.Statics();
            internal Queue<Load>.Statics Queue { get; private set; } 
                = new Queue<Load>.Statics();
            internal FIFOServer<Load>.Statics Server1 { get; private set; } 
                = new FIFOServer<Load>.Statics();
            internal Queue<Load>.Statics Buffer { get; private set; }
                = new Queue<Load>.Statics();
            internal FIFOServer<Load>.Statics Server2 { get; private set; } 
                = new FIFOServer<Load>.Statics();
            
            public Func<Random, TimeSpan> InterArrivalTime { get { return Generator.InterArrivalTime; } set { Generator.InterArrivalTime = value; } }
            public int ServerCapacity1 { get { return Server1.Capacity; } set { Server1.Capacity = value; } }
            public Func<Load, Random, TimeSpan> ServiceTime1 { get { return Server1.ServiceTime; } set { Server1.ServiceTime = value; } }
            public int BufferSize { get { return Buffer.Capacity; } set { Buffer.Capacity = value; } }
            public int ServerCapacity2 { get { return Server2.Capacity; } set { Server2.Capacity = value; } }
            public Func<Load, Random, TimeSpan> ServiceTime2 { get { return Server2.ServiceTime; } set { Server2.ServiceTime = value; } }
            public Func<Load, bool> ToDepart { get { return Server2.ToDepart; } set { Server2.ToDepart = value; } }            
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
            public TwoFIFOServers System { get; private set; }
            public Load Load { get; private set; }
            public ArchiveEvent(TwoFIFOServers system, Load load)
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

        public TwoFIFOServers(Statics config, int seed = 0, string tag = null) : base(config, seed, tag)
        {
            Name = "TwoFIFOServerSystem";
            Processed = new List<Load>();

            Config.Generator.Create = rs => new Load();
            Generator = new Generator<Load>(
                config: Config.Generator,
                seed: DefaultRS.Next());
            Generator.OnArrive.Add(load => Queue.Enqueue(load));

            Queue = new Queue<Load>(
                 config: Config.Queue,
                 tag: "Queue");
            Queue.Config.ToDequeue = load => Server1.Vancancy > 0;
            Queue.OnDequeue.Add(load => Server1.Start(load));

            Server1 = new FIFOServer<Load>(
               config: Config.Server1,
               seed: DefaultRS.Next(),
               tag: "1st Server");
            Server1.Config.ToDepart = load => Buffer.Vancancy > 0;
            Server1.OnDepart.Add(load => Buffer.Enqueue(load));
            Server1.OnDepart.Add(load => Queue.Dequeue());

            Buffer = new Queue<Load>(
                 config: Config.Buffer,
                 tag: "Buffer");
            Buffer.Config.ToDequeue = load => Server2.Vancancy > 0;
            Buffer.OnDequeue.Add(load => Server2.Start(load));
            Buffer.OnDequeue.Add(load => Server1.Depart());

            Server2 = new FIFOServer<Load>(
               config: Config.Server2,
               seed: DefaultRS.Next(),
               tag: "2st Server");
            Server2.Config.ToDepart = load => true;
            Server2.OnDepart.Add(load => new ArchiveEvent(this, load));
            Server2.OnDepart.Add(load => Buffer.Dequeue());

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
