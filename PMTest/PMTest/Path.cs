using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace Test
{
    public class Path : Component<Path.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public ControlPoint Start { get; set; }
            public ControlPoint End { get; set; }
            public double Length { get; set; }
            public int index;
        }
        //public new Statics Config { get { return (Statics)base.Config; } } // for inheritated component       
        #endregion

        #region Sub-Components
        //private Server<TLoad> Server { get; set; }

        #endregion

        #region Dynamics
        public HashSet<Vehicle> OnPath { get; set; } = new HashSet<Vehicle>();
        #endregion

        #region Events
        private abstract class EventOfSegment : Event { internal Path This { get; set; } } // event adapter 
                                                                                              //private class InternalEvent : EventOfSegment
                                                                                              //{
                                                                                              //    internal TLoad Load { get; set; }
                                                                                              //    public override void Invoke() {  }
                                                                                              //}


        private class EnterEvent : Event
        {
            public Path Path { get; private set; }
            public Vehicle Vehicle { get; set; }
            internal EnterEvent(Vehicle vehicle, Path path)
            {
                Path = path;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                Path.OnPath.Add(Vehicle);
            }
            public override string ToString()
            {
                return string.Format("{0}_Enter",Path);
            }
        }

        private class ExitEvent : Event
        {
            public Path Path { get; private set; }
            public Vehicle Vehicle { get; set; }
            internal ExitEvent(Vehicle vehicle, Path path)
            {
                Path = path;
                Vehicle = vehicle;
            }
            public override void Invoke()
            {
                if (Path.OnPath == null) { }//need exception here
                if (!Path.OnPath.Contains(Vehicle)) { }//need exception here
                Path.OnPath.Remove(Vehicle);
            }
            public override string ToString()
            {
                return string.Format("{0}_Exit",Path);
            }
        } // how to write output Events?
        #endregion

        #region Input Events - Getters
        //public Event Input(TLoad load) { return new InternalEvent { This = this, Load = load }; }
        internal Event Enter(Vehicle Vehicle) { return new EnterEvent(Vehicle, this); }
        #endregion

        #region Output Events - Reference to Getters
        //public List<Func<TLoad, Event>> OnOutput { get; private set; } = new List<Func<TLoad, Event>>();
        #endregion


        public Path(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Path";
        }

        public override void WarmedUp(DateTime clockTime)
        {
            throw new NotImplementedException();
        }
    }
}
