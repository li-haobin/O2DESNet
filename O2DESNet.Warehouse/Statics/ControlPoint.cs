using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.Statics
{
    public class ControlPoint
    {
        protected static int _count = 0;
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
        private double GetDistanceToAdjacent(ControlPoint next)
        {
            if (!PathingTable.ContainsKey(next))
                throw new Exceptions.InfeasibleTravelling(
                    "Make sure the next control point is in pathing table.");
            var path = PathingTable[next];
            return Math.Abs(next.Positions[path] - Positions[path]);
        }

        // TODO: Generic distance routing with djikstra. Without running the whole initialisation
        public double GetDistanceTo(ControlPoint destination)
        {
            if (!RoutingTable.ContainsKey(destination))
                throw new Exceptions.InfeasibleTravelling(
                    "Make sure the destination control point is in routing table.");

            double dist = 0;
            var cur = this;

            while (cur != destination)
            {
                var next = cur.RoutingTable[destination];
                dist += cur.GetDistanceToAdjacent(next);
                cur = next;
            }

            return dist;
        }

        // Need another method GetDistanceTo which exploits the structure of a warehouse
        // This is for quicker generation, without having to calculate Dijkstra graph
    }
}
