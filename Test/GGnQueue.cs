using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace Test
{
    public class GGnQueue<TScenario, TStatus, TLoad> : Component
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TLoad : Load<TScenario, TStatus>
    {
        #region Sub-Components
        public Generator<TScenario, TStatus, TLoad> Generator { get; private set; }
        public Queue<TScenario, TStatus, TLoad> Queue { get; private set; }
        public Server<TScenario, TStatus, TLoad> Server1 { get; private set; }
        public Server<TScenario, TStatus, TLoad> Server2 { get; private set; }
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

        #region Dynamic Properties        
        public int NCompleted { get { return (int)Server2.HourCounter.TotalDecrementCount; } }
        #endregion

        #region Input Events - Generators
        public Event<TScenario, TStatus> Start() { return Generator.Start(); }
        public Event<TScenario, TStatus> End() { return Generator.End(); }
        #endregion

        #region Output Events - Reference to Event Generators
        public List<Func<TLoad, Event<TScenario, TStatus>>> OnDepart { get { return Server2.OnDepart; } }
        #endregion
        
        public GGnQueue(StaticProperties statics, int seed, string tag = null): base(seed, tag)
        {
            Name = "GGnQueue";
            Statics = statics;
            
            Generator = new Generator<TScenario, TStatus, TLoad>(
                statics: new Generator<TScenario, TStatus, TLoad>.StaticProperties {
                    Create = Statics.Create,
                    InterArrivalTime = Statics.InterArrivalTime,
                    SkipFirst = false,
                },
                seed: DefaultRS.Next());
            Generator.OnArrive.Add(Queue.Enqueue);

            Queue = new Queue<TScenario, TStatus, TLoad>(
                statics: new Queue<TScenario, TStatus, TLoad>.StaticProperties
                {
                    ToDequeue = () => Server1.Vancancy > 0,
                },
                tag: "Queue");
            Queue.OnDequeue.Add(Server1.Start);

            Server1 = new Server<TScenario, TStatus, TLoad>(
                statics: new Server<TScenario, TStatus, TLoad>.StaticProperties
                {
                    Capacity = Statics.ServerCapacity,
                    ServiceTime = Statics.ServiceTime,
                    ToDepart = () => Server2.Vancancy > 0,
                },
                seed: DefaultRS.Next(),
                tag: "1nd Server");
            Server1.OnDepart.Add(load => Queue.Dequeue());
            Server1.OnDepart.Add(Server2.Start);

            Server2 = new Server<TScenario, TStatus, TLoad>(
               statics: new Server<TScenario, TStatus, TLoad>.StaticProperties
               {
                   Capacity = Statics.ServerCapacity,
                   ServiceTime = Statics.ServiceTime,
                   ToDepart = () => true,
               },
               seed: DefaultRS.Next(),
               tag: "2nd Server");
            Server2.OnDepart.Add(load => Server1.Depart());
        }
        public override void WarmedUp(DateTime clockTime)
        {
            Generator.WarmedUp(clockTime);
            Queue.WarmedUp(clockTime);
            Server2.WarmedUp(clockTime);
        }
        public override void WriteToConsole()
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
