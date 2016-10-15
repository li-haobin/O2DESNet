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
        private class TravelEvent : Event
        {
            public PathMoverSystem PathMoverSystem { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal TravelEvent(PathMoverSystem pathMoverSystem, Vehicle vehicle)
            {
                PathMoverSystem = pathMoverSystem;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                Vehicle.Log(this);
                if (Vehicle.Current == null) Execute(Vehicle.PutOn(GetControlPoint()));
                if (Vehicle.OnComplete.Count == 0) Vehicle.OnComplete.Add(() => new ScheduleEvent(PathMoverSystem, Vehicle));

                Execute(Vehicle.MoveTo(new List<ControlPoint> { GetControlPoint(), GetControlPoint() }));
                
            }
            private ControlPoint GetControlPoint()
            {
                return PathMoverSystem.PathMover.ControlPoints.Values.ElementAt(PathMoverSystem.DefaultRS.Next(PathMoverSystem.PathMover.ControlPoints.Count));
            }
            public override string ToString() { return string.Format("{0}_Travel", PathMoverSystem); }
        }
        private class ScheduleEvent : Event
        {
            public PathMoverSystem PathMoverSystem { get; private set; }
            public Vehicle Vehicle { get; private set; }
            internal ScheduleEvent(PathMoverSystem pathMoverSystem, Vehicle vehicle)
            {
                PathMoverSystem = pathMoverSystem;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                Schedule(new TravelEvent(PathMoverSystem, Vehicle), TimeSpan.FromSeconds(5));
            }
            public override string ToString() { return string.Format("{0}_Schedule", PathMoverSystem); }
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
            InitEvents.Add(new ScheduleEvent(this,
                new Vehicle(new Vehicle.Statics { Speed = 20, KeepTrack = true }, DefaultRS.Next())
                ));
            InitEvents.Add(new ScheduleEvent(this,
                new Vehicle(new Vehicle.Statics { Speed = 2.5, KeepTrack = true }, DefaultRS.Next())
                ));
            InitEvents.Add(new ScheduleEvent(this,
                new Vehicle(new Vehicle.Statics { Speed = 50, KeepTrack = true }, DefaultRS.Next())
                ));
        }

        public override void WarmedUp(DateTime clockTime)
        {
            PathMover.WarmedUp(clockTime);
        }

        public override void WriteToConsole()
        {
            PathMover.WriteToConsole();
        }
    }
}
