using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample.Dynamics
{
    public class GridStatus : PMDynamics
    {
        public GridStatus(Grid grid) : base(grid) { }

        public override void UpdateSpeeds(Path path, DateTime clockTime)
        {
            //base.UpdateSpeeds(path, clockTime);
            foreach (var v in VehiclesOnPath[path]) v.SetSpeed(path.FullSpeed / VehiclesOnPath[path].Count, clockTime);
        }
    }
}
