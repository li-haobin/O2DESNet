using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Traffic;

namespace O2DESNet.Demos.PMTraffic
{
    public class Testbed_PathMover : Module<Testbed_PathMover.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public PathMover.Statics PathMover { get; set; }
            public List<ControlPoint.Statics> Origins { get; set; }
            public List<ControlPoint.Statics> Destinations { get; set; }
            public Vehicle.Statics VehicleCategory { get; set; }
            public int NVehicles { get; set; } = 1;
            public bool RestrictedNeighboringJobs { get; set; } = false;
        }
        #endregion

        #region Sub-Components
        //private Server<TLoad> Server { get; set; }
        #endregion

        #region Dynamics
        public PathMover PathMover { get; private set; }
        public List<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();
        public HourCounter JobsCounter { get; private set; } = new HourCounter();
        //public int Occupancy { get { return Server.Occupancy; } }  
        public HourCounter DeadlocksCounter { get; private set; } = new HourCounter();
        #endregion

        #region Events
        private abstract class EventOfTestbed_PathMover : Event { internal Testbed_PathMover This { get; set; } } // event adapter 

        private class StartEvent : EventOfTestbed_PathMover
        {
            internal Vehicle Vehicle { get; set; }
            internal ControlPoint.Statics At { get; set; } = null;
            public override void Invoke()
            {
                if (At == null) At = This.Config.Origins[DefaultRS.Next(This.Config.Origins.Count)];

                bool neighboring = DefaultRS.NextDouble() < 0.95;
                ControlPoint.Statics target;

                while (true)
                {
                    if (This.Config.Origins.Contains(At))
                        target = This.Config.Destinations[DefaultRS.Next(This.Config.Destinations.Count)];
                    else
                        target = This.Config.Origins[DefaultRS.Next(This.Config.Origins.Count)];

                    if (!This.Config.RestrictedNeighboringJobs) break;
                    var nCols = Math.Abs(Convert.ToInt32(At.Tag.Split('_').Last()) - Convert.ToInt32(target.Tag.Split('_').Last()));
                    if (neighboring && nCols <= 1) break;
                    else if (!neighboring && nCols > 1) break;
                }

                Execute(Vehicle.SetTargets(new List<ControlPoint.Statics> { target }));
                Execute(This.PathMover.CallToDepart(Vehicle, At));
                This.JobsCounter.ObserveChange(1, ClockTime);
            }
        }

        private class FinishEvent : EventOfTestbed_PathMover
        {
            internal Vehicle Vehicle { get; set; }
            internal ControlPoint.Statics At { get; set; }
            public override void Invoke()
            {
                This.JobsCounter.ObserveChange(-1, ClockTime);
                Execute(new StartEvent { This = This, Vehicle = Vehicle, At = At });
            }
        }

        private class DeadLockEvent : EventOfTestbed_PathMover
        {
            public override void Invoke()
            {
                This.DeadlocksCounter.ObserveChange(1, ClockTime);
                string paths = "";
                foreach (var p in This.PathMover.Paths.Values.Where(p => p.Occupancy > 0)) paths += string.Format("{0},", p);
                //Console.Write(".");
                //Console.WriteLine(string.Format("Deadlock Occurs at Path #{0}.", paths.Substring(0, paths.Length - 1)));

                Execute(This.PathMover.Reset());
                foreach (var vehicle in This.Vehicles) Execute(new StartEvent { This = This, Vehicle = vehicle });
            }
        }

        private class TestUpdToArriveEvent : EventOfTestbed_PathMover
        {
            public override void Invoke()
            {
                int idxCP = 10;
                //Schedule(This.PathMover.UpdToArrive(This.PathMover.ControlPoints.Values.ElementAt(idxCP), false), TimeSpan.FromHours(3));
                //Schedule(This.PathMover.UpdToArrive(This.PathMover.ControlPoints.Values.ElementAt(idxCP), true), TimeSpan.FromHours(5));
            }
        }
        #endregion
        
        public Testbed_PathMover(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Testbed_PathMover";
            PathMover = new PathMover(config.PathMover, DefaultRS.Next());
            for (int i = 0; i < config.NVehicles; i++) Vehicles.Add(new Vehicle(config.VehicleCategory, DefaultRS.Next()));
            PathMover.OnArrive.Add((veh, cp) => new FinishEvent { This = this, Vehicle = veh, At = cp });
            PathMover.OnDeadlock.Add(pm => new DeadLockEvent { This = this });
            InitEvents.AddRange(PathMover.InitEvents);
            foreach (var veh in Vehicles) InitEvents.Add(new StartEvent { This = this, Vehicle = veh });

            InitEvents.Add(new TestUpdToArriveEvent { This = this });
        }

        public override void WarmedUp(DateTime clockTime)
        {
            PathMover.WarmedUp(clockTime);
            //foreach (var veh in Vehicles) veh.WarmedUp(clockTime);
            JobsCounter.WarmedUp(clockTime);
            DeadlocksCounter.WarmedUp(clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime = default(DateTime?))
        {
            Console.WriteLine(clockTime);
            PathMover.WriteToConsole();

            Console.WriteLine();
            foreach (var veh in Vehicles) if (veh.Targets.Count > 0) Console.WriteLine("{0}:\t Target CP{1}", veh, veh.Targets.First().Index);
        }
    }
}
