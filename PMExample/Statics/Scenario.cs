using O2DESNet.PathMover;
using PMExample.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample
{
    public class Scenario : O2DESNet.Scenario
    {
        public PMStatics PM { get; protected set; }
        public ControlPoint[] QuayPoints { get; protected set; }
        public ControlPoint[] YardPoints { get; protected set; }

        public int NumVehicles { get; protected set; }
        public double DischargingRatio { get; set; }

        protected Scenario() { }
        public Scenario(double[] colSpaces, double[] rowSpaces, double fullSpeed, int numVehicles)
        {
            var grid = new Grid(colSpaces, rowSpaces, fullSpeed);
            QuayPoints = grid.RowPaths[0].Select(p => grid.CreateControlPoint(p, p.Length / 2)).ToArray();
            YardPoints = Enumerable.Range(1, grid.RowPaths.Length - 1).SelectMany(i => grid.RowPaths[i].Select(p => grid.CreateControlPoint(p, p.Length / 2))).ToArray();
            NumVehicles = numVehicles;
            PM = grid;
        }        
    }
}
