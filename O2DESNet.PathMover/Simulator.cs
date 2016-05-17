using O2DESNet.PathMover.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            Status.PutOn(Scenario.ControlPoints[3], ClockTime).SetSpeed(10);
            foreach (var v in Status.Vehicles) Schedule(
                new Move(v, Scenario.ControlPoints[DefaultRS.Next(Scenario.ControlPoints.Count)],
                () => { Console.WriteLine("FINISHED!"); }), ClockTime);
            //Schedule(new Start(), ClockTime);
        }
    }
}
