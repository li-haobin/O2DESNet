using System;
using System.Collections.Generic;

namespace O2DESNet.PathMover
{
    public class ControlPoint
    {
        public PMScenario Scenario { get; private set; }
        public int Id { get; private set; }
        /// <summary>
        /// Check for the position on each path
        /// </summary>
        public Dictionary<Path, double> Positions { get; internal set; }
        /// <summary>
        /// Check for the next control point to visit, providing the destination
        /// </summary>
        public Dictionary<ControlPoint, ControlPoint> RoutingTable { get; internal set; }
        /// <summary>
        /// Check for the path to take, providing the next control point to visit
        /// </summary>
        internal Dictionary<ControlPoint, Path> PathingTable { get; set; }

        internal ControlPoint(PMScenario scenario)
        {
            Scenario = scenario;
            Id = Scenario.ControlPoints.Count;
            Positions = new Dictionary<Path, double>();
        }
        
        /// <summary>
        /// Get distance to an adjacent control point
        /// </summary>
        public double GetDistanceTo(ControlPoint next)
        {
            if (next.Equals(this)) return 0;
            if (!PathingTable.ContainsKey(next))
                throw new Exception("Make sure the next control point is in pathing table.");
            var path = PathingTable[next];
            return Math.Abs(next.Positions[path] - Positions[path]);
        }

        public override string ToString()
        {
            return string.Format("CP{0}", Id);
        }

    }
}
