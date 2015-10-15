using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Statics
{
    public class ControlPoint
    {
        private static int _count = 0;
        public int Id { get; private set; }
        /// <summary>
        /// Check for the position on each path
        /// </summary>
        public Dictionary<Path, double> Positions { get; internal set; }
        /// <summary>
        /// Check for the next control point to visit, providing the destination
        /// </summary>
        internal Dictionary<ControlPoint, ControlPoint> RoutingTable { get; set; }
        /// <summary>
        /// Check for the path to take, providing the next control point to visit
        /// </summary>
        internal Dictionary<ControlPoint, Path> PathingTable { get; set; }
        internal ControlPoint() { Id = ++_count; Positions = new Dictionary<Path, double>(); }
        /// <summary>
        /// Get distance to an adjacent control point
        /// </summary>
        public double GetDistanceTo(ControlPoint next)
        {
            if (!PathingTable.ContainsKey(next))
                throw new Exceptions.InfeasibleTravelling(
                    "Make sure the next control point is in pathing table.");
            var path = PathingTable[next];
            return Math.Abs(next.Positions[path] - Positions[path]);
        }

    }
}
