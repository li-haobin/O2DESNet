using O2DESNet.PathMover.Statics;
using System;

namespace O2DESNet.PathMover.Dynamics
{
    public class Job
    {
        private static int _count = 0;
        public int Id { get; private set; }
        public Vehicle Vehicle { get; private set; }
        public ControlPoint From { get; private set; }
        public ControlPoint To { get; private set; }
        public Job(DateTime clockTime, Vehicle vehicle, ControlPoint from, ControlPoint to)
        {
            Vehicle = vehicle; From = from; To = to; Id = ++_count;
            Vehicle.PutOn(clockTime, from);
        }
    }
}
