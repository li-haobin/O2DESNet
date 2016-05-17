using O2DESNet;
using O2DESNet.PathMover;
using PMExample.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample
{
    public class Simulator : Simulator<Scenario, Status>
    {
        public Simulator(Status status) : base(status)
        {
            Schedule(new JobStart { Job = Status.CreateJob() }, ClockTime);

            //Func<ControlPoint> getCP = () => Scenario.PM.ControlPoints[DefaultRS.Next(Scenario.PM.ControlPoints.Count)];
            //for (int i = 0; i < 10; i++) Status.PM.PutOn(getCP(), ClockTime);
            //foreach (var v in Status.PM.Vehicles)
            //{
            //    v.Targets = new List<ControlPoint> { getCP(), getCP(), getCP(), };
            //    v.OnCompletion = () => { Console.WriteLine("{0} FINISHED!", v); };
            //    Schedule(new Move { Dynamics = Status.PM, Vehicle = v }, ClockTime);
            //}
        }
    }
}
