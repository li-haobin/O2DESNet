using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Drawing
{
    /// <summary>
    /// The object passed by module to update trajectory of its movement
    /// </summary>
    public class TrajInfo
    {
        /// <summary>
        /// Whether to rewrite previous trjectory information.
        /// Set to false by default.
        /// </summary>
        public bool Rewrite { get; set; } = false;
        /// <summary>
        /// The trajectory that consists of a list of spatio temporal points
        /// </summary>
        public List<SpatioTemporalPoint> Trajectory { get; set; }
    }
    public class SpatioTemporalPoint
    {
        public DateTime Time { get; set; }
        public double? X { get; set; } = null;
        public double? Y { get; set; } = null;
        public double? Z { get; set; } = null;
    }
}
