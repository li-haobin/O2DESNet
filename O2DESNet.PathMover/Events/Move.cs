using O2DESNet.PathMover.Dynamics;
using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Events
{
    internal class Move : Event
    {
        internal Vehicle Vehicle { get; private set; }
        internal Move(Simulator sim, Vehicle vehicle) : base(sim) { Vehicle = vehicle; }
        public override void Invoke()
        {
            if (!Vehicle.On) Vehicle.PutOn(_sim.Scenario.ControlPoints[_sim.RS.Next(_sim.Scenario.ControlPoints.Count)]);
            else Vehicle.Reach();

            var cps = Vehicle.LastControlPoint.PathingTable.Keys.Except(new ControlPoint[] { Vehicle.LastControlPoint }).ToList();
            bool withinBuffer;
            //Vehicle.MoveToNext(cps[_sim.RS.Next(cps.Count)],
                //targetSpeed: 20);
                //targetSpeed: 15.0 + 5.0 * _sim.RS.NextDouble());
                //targetSpeed: 20.0 * _sim.RS.NextDouble());
            //Vehicle.MoveToNext(cps[_sim.RS.Next(cps.Count)], targetSpeed: 20.0 * _sim.RS.NextDouble());
            Vehicle.MoveToNext(cps[_sim.RS.Next(cps.Count)], bufferTime: 0.01, withinBuffer: out withinBuffer);
            if (!withinBuffer) _sim.Status.Count_FailureToScheduleWithinBuffer++;
            if (Vehicle.IdentifyConflicts_PassOver())
            {
                foreach (var i in _sim.Status.Conflicts_PassOver[Vehicle])
                    for (int j = 0; j < i.Value.Length; j++)
                    {
                        if (j / 2 * 2 == j) _sim.ScheduleEvent(new PassOver(_sim, Vehicle, i.Key), i.Value[j]);
                        else _sim.ScheduleEvent(new PassOver(_sim, i.Key, Vehicle), i.Value[j]);
                    }
            }
            if (Vehicle.IdentifyConflicts_CrossOver())
                foreach (var i in _sim.Status.Conflicts_CrossOver[Vehicle])
                    _sim.ScheduleEvent(new CrossOver(_sim, Vehicle, i.Key), i.Value);
            _sim.ScheduleEvent(new Move(_sim, Vehicle), TimeSpan.FromSeconds(Vehicle.Time_ToNextControlPoint));
        }
    }
}
