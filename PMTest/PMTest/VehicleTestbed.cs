using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace Test
{
    public class VehicleTestbed : Component<VehicleTestbed.Statics>
    {
        #region Statics
        public class Statics : Scenario { }
        //public new Statics Config { get { return (Statics)base.Config; } } // for inheritated component
        #endregion

        #region Sub-Components
        //private Server<TLoad> Server { get; set; }
        #endregion

        #region Dynamics
        //public int Occupancy { get { return Server.Occupancy; } }  
        public Vehicle Vehicle { get; private set; }
        #endregion

        #region Events
        private abstract class EventOfVehicleTestbed : Event { internal VehicleTestbed This { get; set; } } // event adapter 

        private class InitEvent : EventOfVehicleTestbed
        {
            public override void Invoke()
            {
                Execute(This.Vehicle.SetSpeed(100));
                Schedule(new ReduceSpeedEvent { This = This, Ratio = 0.01 }, TimeSpan.FromHours(1));
            }
        }
        private class ReduceSpeedEvent : EventOfVehicleTestbed
        {
            public double Ratio { get; set; }
            public override void Invoke()
            {
                Execute(This.Vehicle.SetSpeed(This.Vehicle.Speed * (1 - Ratio)));
                Schedule(new ReduceSpeedEvent { This = This, Ratio = Ratio }, TimeSpan.FromHours(2));
            }
        }
        #endregion

        #region Input Events - Getters
        //public Event Input(TLoad load) { return new InternalEvent { This = this, Load = load }; }
        #endregion

        #region Output Events - Reference to Getters
        //public List<Func<TLoad, Event>> OnOutput { get; private set; } = new List<Func<TLoad, Event>>();
        #endregion

        public VehicleTestbed(Statics config, int seed = 0, string tag = null) : base(config, seed, tag)
        {
            Name = "VehicleTestbed";
            Vehicle = new Vehicle(new Vehicle.Statics(), 0);
            
            InitEvents.Add(new InitEvent { This = this });
        }

        public override void WarmedUp(DateTime clockTime)
        {
            throw new NotImplementedException();
        }

        public override void WriteToConsole(DateTime? clockTime = default(DateTime?))
        {
            Console.WriteLine(clockTime);
            Console.WriteLine("===============================");
            Vehicle.WriteToConsole(clockTime);
        }

        static void Main()
        {
            var sim = new Simulator(new VehicleTestbed(new Statics()));
            while (true)
            {
                sim.Run(TimeSpan.FromDays(0.1));
                sim.WriteToConsole();
                Console.ReadKey();
                Console.WriteLine();
            }
        }
    }
}
