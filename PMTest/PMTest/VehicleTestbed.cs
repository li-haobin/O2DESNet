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
        List<ControlPoint> Targets1 = new List<ControlPoint> { (new ControlPoint(new ControlPoint.Statics { }, seed: 0)), (new ControlPoint(new ControlPoint.Statics { }, seed: 0)), (new ControlPoint(new ControlPoint.Statics { }, seed: 0)) };
        List<ControlPoint> Targets2 = new List<ControlPoint> { (new ControlPoint(new ControlPoint.Statics { }, seed: 0)), (new ControlPoint(new ControlPoint.Statics { }, seed: 0)), (new ControlPoint(new ControlPoint.Statics { }, seed: 0)) };
        Path Path1 = new Path(new Path.Statics{},seed: 0);
        Path Path2 = new Path(new Path.Statics { }, seed: 0);
        Path Path3 = new Path(new Path.Statics { }, seed: 0);
        Path Path4 = new Path(new Path.Statics { }, seed: 0);
        int count = 2;
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
                //Execute(This.Vehicle.SetSpeed(0));
                //Execute(This.Vehicle.SetAcceleration(5));
                //Schedule(new ReduceSpeedEvent { This = This, Ratio = 0.01 }, TimeSpan.FromHours(1));
                //Schedule(new AccelerateEvent { This = This, Acceleration = 1 }, TimeSpan.FromSeconds(5));
                //Schedule(new DecelerateEvent { This = This, Acceleration = -1 }, TimeSpan.FromSeconds(3));
                //Schedule(new ReduceAccelerationEvent { This = This, Ratio = 0.02 }, TimeSpan.FromSeconds(1));

                //Execute(This.Vehicle.SetTargets(This.Targets1));
                //Schedule(new ChangeTargetsEvent { This = This, Targets = This.Targets2},TimeSpan.FromHours(0.5));
                //Schedule(new AddTargetsEvent { This = This, NewTargets = This.Targets2},TimeSpan.FromHours(1));
                //Schedule(new SetPositionEvent { This = This, Position = This.Vehicle.Targets[0] }, TimeSpan.FromHours(1));

                //Execute(This.Vehicle.SetTravellingPath(This.Path1));
                //Schedule(new SetTravellingPathEvent { This = This, TravellingOn = This.Path2 }, TimeSpan.FromHours(0.1));

                Execute(This.Vehicle.UpdateMileage(50));
                Schedule(new TestMileageEvent { This = This, Mileage = 50, Reset = 1}, TimeSpan.FromHours(1));
                
            }
        }
        private class AccelerateEvent : EventOfVehicleTestbed
        {
            public double Acceleration { get; set; }
            public override void Invoke()
            {
                Execute(This.Vehicle.SetAcceleration(Acceleration));
                Execute(This.Vehicle.SetSpeed(This.Vehicle.Speed + Acceleration));
                Schedule(new AccelerateEvent { This = This, Acceleration = Acceleration }, TimeSpan.FromSeconds(1));
            }
        }       
        private class DecelerateEvent : EventOfVehicleTestbed
        {
            public double Acceleration { get; set; }
            public override void Invoke()
            {
                Execute(This.Vehicle.SetAcceleration(Acceleration));
                Execute(This.Vehicle.SetSpeed(This.Vehicle.Speed + Acceleration*2.5));
                Schedule(new DecelerateEvent { This = This, Acceleration =  Acceleration }, TimeSpan.FromSeconds(2.5));
            }
        }
        private class ReduceAccelerationEvent : EventOfVehicleTestbed
        {
            public double Ratio { get; set; }
            public override void Invoke()
            {
                Execute(This.Vehicle.SetAcceleration(This.Vehicle.Acceleration*(1-Ratio)));
                Execute(This.Vehicle.SetSpeed(This.Vehicle.Speed + This.Vehicle.Acceleration));
                Schedule(new ReduceAccelerationEvent { This = This, Ratio = Ratio}, TimeSpan.FromSeconds(1));
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
        private class ChangeTargetsEvent : EventOfVehicleTestbed
        {
            public List<ControlPoint> Targets { get; set; }
            public override void Invoke()
            { 
                Execute(This.Vehicle.SetTargets(Targets));
                if (This.Vehicle.Targets.SequenceEqual(This.Targets1))
                    Schedule(new ChangeTargetsEvent { This = This, Targets = new List<ControlPoint>(This.Targets2.ToArray()) }, TimeSpan.FromHours(1));
                else Schedule(new ChangeTargetsEvent { This = This, Targets = new List<ControlPoint>(This.Targets1.ToArray()) }, TimeSpan.FromHours(1));                
            }
        }
        private class AddTargetsEvent : EventOfVehicleTestbed
        {
            public List<ControlPoint> NewTargets { get; set; }
            public override void Invoke()
            {
                Execute(This.Vehicle.AddTargets(NewTargets));
                if (This.Vehicle.Targets.Count % 2 == 1) Schedule(new AddTargetsEvent { This = This, NewTargets = This.Targets2 }, TimeSpan.FromHours(1));
                else Schedule(new AddTargetsEvent { This = This, NewTargets = This.Targets1}, TimeSpan.FromHours(1));
            }
        }
        private class SetPositionEvent : EventOfVehicleTestbed
        {
            public Random ran = new Random();
            public ControlPoint Position { get; set; }
            public override void Invoke()
            {
                Execute(This.Vehicle.SetTimeStamp(ClockTime));
                if (This.Vehicle.Targets.Count == 1) 
                {
                    Execute(This.Vehicle.AddTargets(This.Targets1));
                    Execute(This.Vehicle.AddTargets(This.Targets2));
                }
                Execute(This.Vehicle.SetPosition(Position));
                Schedule(new SetPositionEvent { This = This, Position = This.Vehicle.Targets[ran.Next(0, This.Vehicle.Targets.Count)] }, TimeSpan.FromHours(0.5));
            }
        }
        private class SetTravellingPathEvent : EventOfVehicleTestbed
        {
            Random rand = new Random();

            public Path TravellingOn { get; set; }
            public override void Invoke()
            {
                //Execute(This.Vehicle.SetTravellingPath(TravellingOn));
                //Execute(This.Vehicle.SetTimeStamp(ClockTime));
                //if (rand.Next(0, 4) % 4 == 0) Schedule(new SetTravellingPathEvent { This = This, TravellingOn = This.Path1 }, TimeSpan.FromHours(0.1));
                //else if (rand.Next(0, 4) % 4 == 1) Schedule(new SetTravellingPathEvent { This = This, TravellingOn = This.Path2 }, TimeSpan.FromHours(0.1));
                //else if (rand.Next(0, 4) % 4 == 2) Schedule(new SetTravellingPathEvent { This = This, TravellingOn = This.Path3 }, TimeSpan.FromHours(0.1));
                //else Schedule(new SetTravellingPathEvent { This = This, TravellingOn = This.Path4 }, TimeSpan.FromHours(0.1));

                Execute(This.Vehicle.SetTravellingPath(TravellingOn));
                Execute(This.Vehicle.SetTimeStamp(ClockTime));
                if (This.count % 4 == 0)
                {
                    Schedule(new SetTravellingPathEvent { This = This, TravellingOn = This.Path1 }, TimeSpan.FromHours(0.1));
                    This.count++;
                }
                else if (This.count % 4 == 1)
                {
                    Schedule(new SetTravellingPathEvent { This = This, TravellingOn = This.Path2 }, TimeSpan.FromHours(0.1));
                    This.count++;
                }
                else if (This.count % 4 == 2)
                {
                    Schedule(new SetTravellingPathEvent { This = This, TravellingOn = This.Path3 }, TimeSpan.FromHours(0.1));
                    This.count++;
                }
                else
                {
                    Schedule(new SetTravellingPathEvent { This = This, TravellingOn = This.Path4 }, TimeSpan.FromHours(0.1));
                    This.count++;
                }
            }
        }
        private class TestMileageEvent : EventOfVehicleTestbed
        {
            public double Mileage { get; set; }
            public int Reset { get; set; }
            public override void Invoke()
            {
                This.count++;
                Execute(This.Vehicle.UpdateMileage(Mileage));
                Execute(This.Vehicle.ResetMileage(Reset));
                Schedule(new TestMileageEvent { This = This, Mileage = Mileage * 0.5, Reset = This.count % 3 + 1 }, TimeSpan.FromHours(1));
            }
        }
        //private class PrintTimeStampEvent : EventOfVehicleTestbed
        //{
        //    public DateTime TimeStamp { get; set; }
        //    public override void Invoke()
        //    {
        //        Execute(This.Vehicle.SetTimeStamp(TimeStamp));
        //        Schedule(new PrintTimeStampEvent { This = This, TimeStamp = ClockTime}, TimeSpan.FromHours(0.1));
        //    }
        //}
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
                //sim.Run(TimeSpan.FromDays(0.1));
                sim.Run(TimeSpan.FromHours(0.5));
                sim.WriteToConsole();
                Console.ReadKey();
                Console.WriteLine();
                //sim.Run(TimeSpan.FromSeconds(3));
                //sim.WriteToConsole();
                //Console.ReadKey();
                //Console.WriteLine();
            }
        }
    }
}
