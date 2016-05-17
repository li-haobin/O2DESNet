using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathMoverUseCase
{
    public class Scenario : O2DESNet.Scenario
    {
        public O2DESNet.PathMover.Scenario PM1 { get; set; }

        public static Scenario GetExample()
        {
            var pm = new O2DESNet.PathMover.Scenario();
            var paths = Enumerable.Range(0, 6).Select(i => pm.CreatePath(length: 100, fullSpeed: 10, direction: Direction.Forward)).ToArray();
            pm.Connect(paths[0], paths[1]);
            pm.Connect(paths[1], paths[2]);
            pm.Connect(paths[2], paths[3]);
            pm.Connect(paths[3], paths[0]);
            pm.Connect(paths[0], paths[4], 50, 0);
            pm.Connect(paths[2], paths[4], 50, 100);
            pm.Connect(paths[1], paths[5], 50, 0);
            pm.Connect(paths[3], paths[5], 50, 100);
            pm.Connect(paths[4], paths[5], 50, 50);
            var cp1 = pm.CreateControlPoint(paths[0], 30);
            var cp2 = pm.CreateControlPoint(paths[0], 40);
            return new Scenario { PM1 = pm };
        }
    }
}
