using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet.Demos.GGnQueue
{
    public class GGnQueue : State<GGnQueue.Statics>
    {
        #region Sub-Components
        public Generator<Load> Generator { get; private set; }
        public Queueing<Load> Queue { get; private set; }
        public Server<Load> Server { get; private set; }
        #endregion

        #region Statics
        public class Statics : Scenario
        {
            internal Generator<Load>.Statics Generator { get; private set; } = new Generator<Load>.Statics();
            internal Queueing<Load>.Statics Queue { get; private set; } = new Queueing<Load>.Statics();
            internal Server<Load>.Statics Server { get; private set; } = new Server<Load>.Statics();
            
            public Func<Random, TimeSpan> InterArrivalTime { get { return Generator.InterArrivalTime; } set { Generator.InterArrivalTime = value; } }
            public Func<Load, Random, TimeSpan> ServiceTime { get { return Server.ServiceTime; } set { Server.ServiceTime = value; } }
            public int ServerCapacity { get { return Server.Capacity; } set { Server.Capacity = value; } }            
        }
        #endregion

        #region Dynamics
        public List<Load> Processed { get; private set; }
        public int NCompleted { get { return (int)Server.UtilizationCounter.TotalDecrementCount; } }
        #endregion

        #region Events
        private abstract class InternalEvent : Event<GGnQueue, Statics> { } // event adapter 
        private class EnterEvent : InternalEvent
        {
            internal Load Load { get; set; }
            public override void Invoke()
            {
                Execute(Load.Log(this));
                Execute(This.Queue.Enqueue(Load));
            }
        }
        private class ExitEvent : InternalEvent
        {
            internal Load Load { get; set; }
            public override void Invoke()
            {
                Execute(Load.Log(this));
                This.Processed.Add(Load);
            }
        }
        #endregion

        #region Input Events - Getters
        public Event Start() { return Generator.Start(); }
        public Event End() { return Generator.End(); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Load, Event>> OnDepart { get; } = new List<Func<Load, Event>>();
        #endregion
        
        public GGnQueue(Statics config, int seed = 0, string tag = null) : base(config, seed, tag)
        {
            Name = "GGnQueueSystem";
            Processed = new List<Load>();

            Config.Generator.Create = rs => new Load();
            Generator = new Generator<Load>(Config.Generator, DefaultRS.Next());
            Generator.OnArrive.Add(load => new EnterEvent { This = this, Load = load });

            Queue = new Queueing<Load>(Config.Queue, "Queue");
            Queue.OnDequeue.Add(load => Server.Start(load));

            Server = new Server<Load>(Config.Server, DefaultRS.Next(), "Server");
            Server.OnStateChg.Add(() => Queue.UpdToDequeue(Server.Vacancy > 0));
            Server.OnDepart.Add(load => new ExitEvent { This = this, Load = load });
            Server.OnDepart.Add(load => EventWrapper(OnDepart.Select(e => e(load))));

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
