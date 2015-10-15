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
            if (!Vehicle.On)
                _sim.Status.PutOn(Vehicle, _sim.Scenario.ControlPoints[_sim.RS.Next(_sim.Scenario.ControlPoints.Count)]);
            else _sim.Status.Reach(Vehicle);

            var cps = Vehicle.Current.PathingTable.Keys.Except(new ControlPoint[] { Vehicle.Current }).ToList();
            _sim.Status.MoveTowards(Vehicle, cps[_sim.RS.Next(cps.Count)], targetSpeed: 20.0 * _sim.RS.NextDouble());
            _sim.ScheduleEvent(new Move(_sim, Vehicle), TimeSpan.FromHours(Vehicle.EndingTime));
        }
    }
}
