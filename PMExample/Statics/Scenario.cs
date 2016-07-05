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
        public Grid PM { get; private set; }
        public ControlPoint[] QuayPoints { get; private set; }
        public ControlPoint[] YardPoints { get; private set; }

        public int NumVehicles { get; private set; }
        public double DischargingRatio { get; set; }        

        public Scenario(double[] colSpaces, double[] rowSpaces, double fullSpeed, int numVehicles)
        {
            PM = new Grid(colSpaces, rowSpaces, fullSpeed);
            QuayPoints = PM.RowPaths[0].Select(p => PM.CreateControlPoint(p, p.Length / 2)).ToArray();
            YardPoints = Enumerable.Range(1, PM.RowPaths.Length - 1).SelectMany(i => PM.RowPaths[i].Select(p => PM.CreateControlPoint(p, p.Length / 2))).ToArray();
            NumVehicles = numVehicles;
        }        
    }
}
