using O2DESNet.PathMover;
using PMExample.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample
{
    public class TwoWayScenario : Scenario
    {
        public TwoWayScenario(double[] colSpaces, double[] rowSpaces, double fullSpeed, int numVehicles)
        {
            var grid = new TwoWayGrid(colSpaces, rowSpaces, fullSpeed);
            Func<Path[], ControlPoint> getMidPoint = paths =>
            {
                var cp = grid.CreateControlPoint(paths[0], paths[0].Length / 2);
                grid.Connect(paths[1], paths[1].Length / 2, cp);
                return cp;
            };
            QuayPoints = grid.RowPaths[0].Select(p => getMidPoint(p)).ToArray();
            YardPoints = Enumerable.Range(1, grid.RowPaths.Length - 1).SelectMany(i => grid.RowPaths[i].Select(p => getMidPoint(p))).ToArray();
            NumVehicles = numVehicles;
            PM = grid;
        }        
    }
}
