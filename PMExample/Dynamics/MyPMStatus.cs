using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample.Dynamics
{
    public class MyPMStatus : PMStatus
    {
        public MyPMStatus(PMScenario pm) : base(pm) { }

        public override void UpdateSpeeds(Path path, DateTime clockTime)
        {
            //base.UpdateSpeeds(path, clockTime);
            foreach (var v in VehiclesOnPath[path]) v.SetSpeed(path.FullSpeed / VehiclesOnPath[path].Count, clockTime);
            //foreach (var v in VehiclesOnPath[path]) v.SetSpeed(path.FullSpeed / Math.Pow(VehiclesOnPath[path].Count, 2), clockTime);
        }
    }
}
