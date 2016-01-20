using O2DESNet.PathMover.Dynamics;
using O2DESNet.PathMover.Statics;
using System;
using System.Linq;

namespace O2DESNet.PathMover.Events
{
    internal class Move : Event<Scenario,Status>
    {
        public Vehicle Vehicle { get; set; }
        protected override void Invoke()
        {
            if (!Vehicle.On) Vehicle.PutOn(ClockTime, Scenario.ControlPoints[DefaultRS.Next(Scenario.ControlPoints.Count)]);
            else Vehicle.Reach(ClockTime);

            var cps = Vehicle.LastControlPoint.PathingTable.Keys.Except(new ControlPoint[] { Vehicle.LastControlPoint }).ToList();
            //bool withinBuffer;
            Vehicle.MoveToNext(ClockTime, cps[DefaultRS.Next(cps.Count)], 
                //targetSpeed: 20);
                targetSpeed: 15.0 + 5.0 * DefaultRS.NextDouble());
                //targetSpeed: 20.0 * DefaultRS.NextDouble());
            //Vehicle.MoveToNext(ClockTime, cps[DefaultRS.Next(cps.Count)], bufferTime: 0.01, withinBuffer: out withinBuffer);
            //if (!withinBuffer) Status.Count_FailureToScheduleWithinBuffer++;
            if (Vehicle.IdentifyConflicts_PassOver())
            {
                foreach (var i in Status.Conflicts_PassOver[Vehicle])
                    for (int j = 0; j < i.Value.Length; j++)
                    {
                        if (j / 2 * 2 == j) Schedule(new PassOver { Vehicle1 = Vehicle, Vehicle2 = i.Key }, i.Value[j]);
                        else Schedule(new PassOver { Vehicle1 = i.Key, Vehicle2 = Vehicle }, i.Value[j]);
                    }
            }
            if (Vehicle.IdentifyConflicts_CrossOver())
                foreach (var i in Status.Conflicts_CrossOver[Vehicle])
                    Schedule(new CrossOver { Vehicle1 = Vehicle, Vehicle2 = i.Key }, i.Value);
            Schedule(new Move { Vehicle = Vehicle }, TimeSpan.FromSeconds(Vehicle.Time_ToNextControlPoint));
        }
    }
}
