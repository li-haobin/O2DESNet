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
        public Dictionary<Path, double> Positions { get; internal set; }
        internal Dictionary<ControlPoint, ControlPoint> RouteTable { get; set; }
        internal ControlPoint(int id) { Id = id; Positions = new Dictionary<Path, double>(); }
        
    }
}
