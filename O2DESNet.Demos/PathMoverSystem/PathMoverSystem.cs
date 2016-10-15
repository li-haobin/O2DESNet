using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet.Demos.PathMoverSystem
{
    public class PathMoverSystem : Component<PathMoverSystem.Statics>
    {
        #region Statics
        public class Statics : Scenario {
            public PathMover.Statics PathMover { get; set; }
        }
        #endregion

        #region Sub-Components
        internal PathMover PathMover { get; private set; }
        #endregion

        #region Dynamics
        //public HashSet<TLoad> Serving { get { return H_Server.Serving; } }
        //public List<TLoad> Served { get { return H_Server.Served; } }
        //public HashSet<TLoad> Restoring { get { return R_Server.Serving; } }
        //public int Vancancy { get { return Statics.Capacity - Serving.Count - Served.Count - Restoring.Count; } }
        //public int NCompleted { get { return (int)H_Server.HourCounter.TotalDecrementCount; } }     
        #endregion

        #region Events
        private class TestEvent : Event
        {
            public PathMoverSystem PathMoverSystem { get; private set; }
            public Vehicle Vehicle { get; private set; }
            public ControlPoint From { get; private set; }
            public ControlPoint To { get; private set; }
            internal TestEvent(PathMoverSystem pathMoverSystem, Vehicle vehicle, ControlPoint from, ControlPoint to)
            {
                PathMoverSystem = pathMoverSystem;
                Vehicle = vehicle;
                From = from;
                To = to;
            }
            public override void Invoke()
            {
                Vehicle.Log(this);
                Execute(Vehicle.PutOn(From));
                Execute(Vehicle.Depart(To));
            }
            public override string ToString() { return string.Format("{0}_Test", PathMoverSystem); }
        }
        #endregion

        #region Input Events - Getters
        //public Event Depart() { return H_Server.Depart(); }
        //public Event Start(TLoad load)
        //{
        //    if (Vancancy < 1) throw new HasZeroVacancyException();
        //    if (Statics.HandlingTime == null) throw new HandlingTimeNotSpecifiedException();
        //    if (Statics.RestoringTime == null) throw new RestoringTimeNotSpecifiedException();
        //    if (Statics.ToDepart == null) throw new DepartConditionNotSpecifiedException();
        //    return H_Server.Start(load);
        //}
        //public Event Depart() { return new DepartEvent(this); }
        #endregion

        #region Output Events - Reference to Getters
        //public List<Func<TLoad, Event>> OnDepart { get { return H_Server.OnDepart; } }
        //public List<Func<Event>> OnRestore { get; private set; }
        #endregion

        #region Exeptions
        //public class HasZeroVacancyException : Exception
        //{
        //    public HasZeroVacancyException() : base("Check vacancy of the Server before execute Start event.") { }
        //}
        //public class HandlingTimeNotSpecifiedException : Exception
        //{
        //    public HandlingTimeNotSpecifiedException() : base("Set HandlingTime as a random generator.") { }
        //}
        //public class RestoringTimeNotSpecifiedException : Exception
        //{
        //    public RestoringTimeNotSpecifiedException() : base("Set RestoringTime as a random generator.") { }
        //}
        //public class DepartConditionNotSpecifiedException : Exception
        //{
        //    public DepartConditionNotSpecifiedException() : base("Set ToDepart as depart condition.") { }
        //}
        #endregion

        public PathMoverSystem(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "PathMoverSystem";
            PathMover = new PathMover(Config.PathMover, DefaultRS.Next());

            // initialize event, compulsory if it's assembly
            InitEvents.Add(new TestEvent(this,
                new Vehicle(new Vehicle.Statics { Speed = 20, KeepTrack = true }, DefaultRS.Next()),
                PathMover.ControlPoints.ElementAt(3).Value,
                PathMover.ControlPoints.ElementAt(0).Value
                ));
            InitEvents.Add(new TestEvent(this,
                new Vehicle(new Vehicle.Statics { Speed = 2.5, KeepTrack = true }, DefaultRS.Next()),
                PathMover.ControlPoints.ElementAt(9).Value,
                PathMover.ControlPoints.ElementAt(7).Value
                ));
            InitEvents.Add(new TestEvent(this,
                new Vehicle(new Vehicle.Statics { Speed = 50, KeepTrack = true }, DefaultRS.Next()),
                PathMover.ControlPoints.ElementAt(1).Value,
                PathMover.ControlPoints.ElementAt(0).Value
                ));
        }

        public override void WarmedUp(DateTime clockTime)
        {
            //H_Server.WarmedUp(clockTime);
            //R_Server.WarmedUp(clockTime);
        }

        public override void WriteToConsole()
        {
            //Console.WriteLine("[{0}]", this);
            //Console.Write("Serving: ");
            //foreach (var load in Serving) Console.Write("{0} ", load);
            //Console.WriteLine();
            //Console.Write("Served: ");
            //foreach (var load in Served) Console.Write("{0} ", load);
            //Console.WriteLine();
            //Console.Write("Restoring: ");
            //foreach (var load in Restoring) Console.Write("{0} ", load);
            //Console.WriteLine();
        }
    }
}
