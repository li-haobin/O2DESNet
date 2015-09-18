using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    class ControlPoint
    {
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
        internal ControlPoint(int id) { Id = id; Positions = new Dictionary<Path, double>(); }
        
    }
}
