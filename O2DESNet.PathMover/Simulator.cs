using O2DESNet.PathMover.Events;
using O2DESNet.PathMover.Statics;
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
            Func<ControlPoint> getCP = () => Scenario.ControlPoints[DefaultRS.Next(Scenario.ControlPoints.Count)];
            for (int i = 0; i < 10; i++) Status.PutOn(getCP(), ClockTime);
            foreach (var v in Status.Vehicles)
            {
                v.Targets = new List<ControlPoint> { getCP(), getCP(), getCP(), };
                v.OnCompletion = () => { Console.WriteLine("FINISHED!"); };
                Schedule(new Move { Vehicle = v }, ClockTime);
            }
            //Schedule(new Start(), ClockTime);
        }
    }
}
