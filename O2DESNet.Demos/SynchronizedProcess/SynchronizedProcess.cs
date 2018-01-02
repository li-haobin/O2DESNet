using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet.Demos.SynchronizedProcess
{
    public class SynchronizedProcess : Module<SynchronizedProcess.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public Generator<Load>.Statics GeneratorA { get; set; }
            public Generator<Load>.Statics GeneratorB { get; set; }
            public Queueing<Load>.Statics QueueA { get; set; }
            public Queueing<Load>.Statics QueueB { get; set; }
            public Server<Load>.Statics ServerA1 { get; set; }
            public Server<Load>.Statics ServerA2 { get; set; }
            public Server<Load>.Statics ServerB1 { get; set; }
            public Server<Load>.Statics ServerB2 { get; set; }
        }
        #endregion

        #region Sub-Components
        public Generator<Load> GeneratorA { get; private set; }
        public Generator<Load> GeneratorB { get; private set; }
        public Queueing<Load> QueueA { get; private set; }
        public Queueing<Load> QueueB { get; private set; }
        public Server<Load> ServerA1 { get; private set;}
        public Server<Load> ServerA2 { get; private set; }
        public Server<Load> ServerB1 { get; private set; }
        public Server<Load> ServerB2 { get; private set; }
        public Synchronizer Sync { get; private set; }
        #endregion
        
        public SynchronizedProcess(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "SynchronizedProcess";

            GeneratorA = new Generator<Load>(config.GeneratorA, DefaultRS.Next());
            GeneratorA.OnArrive.Add(load => QueueA.Enqueue(load));

            GeneratorB = new Generator<Load>(config.GeneratorB, DefaultRS.Next());
            GeneratorB.OnArrive.Add(load => QueueB.Enqueue(load));

            QueueA = new Queueing<Load>(config.QueueA, "Queue A");
            QueueA.OnDequeue.Add(load => ServerA1.Start(load));

            QueueB = new Queueing<Load>(config.QueueB, "Queue B");
            QueueB.OnDequeue.Add(load => ServerB1.Start(load));

            ServerA1 = new Server<Load>(config.ServerA1, DefaultRS.Next(), "Server A1");
            ServerA1.OnDepart.Add(load => ServerA2.Start(load));
            ServerA1.OnStateChg.Add(s => QueueA.UpdToDequeue(s.Vacancy > 0));
            ServerA1.OnStateChg.Add(s => Sync.UpdState(1, s.Served.Count > 0)); // condition 1

            ServerB1 = new Server<Load>(config.ServerB1, DefaultRS.Next(), "Server B1");
            ServerB1.OnDepart.Add(load => ServerB2.Start(load));
            ServerB1.OnStateChg.Add(s => QueueB.UpdToDequeue(s.Vacancy > 0));
            ServerB1.OnStateChg.Add(s => Sync.UpdState(2, s.Served.Count > 0)); // condition 2

            Sync = new Synchronizer(new Synchronizer.Statics(4));
            Sync.OnStateChg.Add(s => ServerA1.UpdToDepart(s.AllTrue)); // action 1
            Sync.OnStateChg.Add(s => ServerB1.UpdToDepart(s.AllTrue)); // action 2

            ServerA2 = new Server<Load>(config.ServerA2, DefaultRS.Next(), "Server A2");
            //ServerA2.OnStateChange.Add(s => ServerA1.UpdToDepart(s.Vacancy > 0)); // without sync
            ServerA2.OnStateChg.Add(s => Sync.UpdState(3, s.Vacancy > 0)); // condition 3
            ServerB2 = new Server<Load>(config.ServerB2, DefaultRS.Next(), "Server B2");
            //ServerB2.OnStateChange.Add(s => ServerB1.UpdToDepart(s.Vacancy > 0)); // without sync
            ServerB2.OnStateChg.Add(s => Sync.UpdState(4, s.Vacancy > 0)); // condition 4

            InitEvents.Add(GeneratorA.Start());
            InitEvents.Add(GeneratorB.Start());
            InitEvents.Add(Sync.UpdState(3, ServerA2.Vacancy > 0));
            InitEvents.Add(Sync.UpdState(4, ServerB2.Vacancy > 0));
        }

        public override void WarmedUp(DateTime clockTime)
        {
            //H_Server.WarmedUp(clockTime);
            //R_Server.WarmedUp(clockTime);
        }
        
        public override void WriteToConsole(DateTime? clockTime = default(DateTime?))
        {
            QueueA.WriteToConsole(); Console.WriteLine();
            ServerA1.WriteToConsole(); Console.WriteLine();
            ServerA2.WriteToConsole(); Console.WriteLine();

            QueueB.WriteToConsole(); Console.WriteLine();
            ServerB1.WriteToConsole(); Console.WriteLine();
            ServerB2.WriteToConsole(); Console.WriteLine();
        }
    }
}
