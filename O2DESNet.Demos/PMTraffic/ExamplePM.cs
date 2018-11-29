using O2DESNet.Traffic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.PMTraffic
{
    partial class Program
    {
        static PathMover.Statics ExamplePM(bool crossHatchAtJunctions = true)
        {
            var pm = new PathMover.Statics();
            int nRows = 3, nCols = 4;
            double block_width = 64, block_length = 304, junction_width = 4, junction_length = 12;
            double vehicle_length = 16 + 1.75;

            // Junctions Points
            for (int rowId = 0; rowId <= nRows; rowId++)
            {
                for (int colId = 0; colId <= nCols; colId++)
                {
                    var cp_in = pm.CreateControlPoint(tag: string.Format("I_{0}_{1}", rowId, colId));
                    cp_in.X = colId * (block_length + junction_length);
                    cp_in.Y = rowId == 0 ? 0 : rowId * (block_width + junction_width) + (junction_length - junction_width);

                    var cp_out = pm.CreateControlPoint(tag: string.Format("O_{0}_{1}", rowId, colId));
                    cp_out.X = cp_in.X + junction_length;
                    cp_out.Y = rowId == 0 ? cp_in.Y + junction_length : cp_in.Y + junction_width;

                    Path.Statics path;
                    path = pm.CreatePath(
                        tag: string.Format("J_{0}_{1}", rowId, colId),
                        length: rowId == 0 ? junction_length * 2 : junction_length + junction_width,
                        capacity: 1,
                        start: cp_in,
                        end: cp_out,
                        crossHatched: crossHatchAtJunctions
                        );
                    path.SpeedByDensity = d => SpeedByDensity(0) / 2; // get 1/2 max speed
                }
            }

            // Row Paths & Work Points
            var capacity = (int)Math.Floor(block_length / 2 / vehicle_length);
            for (int rowId = 0; rowId <= nRows; rowId++)
            {
                string init = "T";
                if (rowId == 0) init = "Q";
                else if (rowId == nRows) init = "E";

                for (int colId = 0; colId < nCols; colId++)
                {
                    var cp_work = pm.CreateControlPoint(tag: string.Format("{0}_{1}_{2}", init, rowId, colId)); // Work Points
                    cp_work.X = colId * (block_length + junction_length) + junction_length + block_length / 2;
                    cp_work.Y = rowId == 0 ? 0 : rowId * (block_width + junction_width) + (junction_length - junction_width);

                    Path.Statics path;

                    path = pm.CreatePath(
                        tag: string.Format("P_{0}_{1}_R0", rowId, colId),
                        length: block_length / 2,
                        capacity: capacity,
                        start: pm.ControlPoints[string.Format("O_{0}_{1}", rowId, colId)],
                        end: cp_work
                        );
                    if (rowId == 0) path.SpeedByDensity = d => SpeedByDensity(d / 2);
                    else path.SpeedByDensity = SpeedByDensity;

                    path = pm.CreatePath(
                        tag: string.Format("P_{0}_{1}_L0", rowId, colId + 1),
                        length: block_length / 2,
                        capacity: capacity,
                        start: pm.ControlPoints[string.Format("O_{0}_{1}", rowId, colId + 1)],
                        end: cp_work
                        );
                    if (rowId == 0) path.SpeedByDensity = d => SpeedByDensity(d / 2);
                    else path.SpeedByDensity = SpeedByDensity;

                    path = pm.CreatePath(
                        tag: string.Format("P_{0}_{1}_L1", rowId, colId),
                        length: block_length / 2,
                        capacity: capacity,
                        start: cp_work,
                        end: pm.ControlPoints[string.Format("I_{0}_{1}", rowId, colId)]
                        );
                    if (rowId == 0) path.SpeedByDensity = d => SpeedByDensity(d / 2);
                    else path.SpeedByDensity = SpeedByDensity;

                    path = pm.CreatePath(
                        tag: string.Format("P_{0}_{1}_R1", rowId, colId + 1),
                        length: block_length / 2,
                        capacity: capacity,
                        start: cp_work,
                        end: pm.ControlPoints[string.Format("I_{0}_{1}", rowId, colId + 1)]
                        );
                    if (rowId == 0) path.SpeedByDensity = d => SpeedByDensity(d / 2);
                    else path.SpeedByDensity = SpeedByDensity;

                }
            }

            // Col Paths, 2 Lanes each
            capacity = (int)Math.Floor(block_width / vehicle_length * 2);
            for (int rowId = 0; rowId < nRows; rowId++)
            {
                for (int colId = 0; colId <= nCols; colId++)
                {
                    Path.Statics path;

                    path = pm.CreatePath(
                        tag: string.Format("P_{0}_{1}_D", rowId, colId),
                        length: block_length / 2,
                        capacity: capacity,
                        start: pm.ControlPoints[string.Format("O_{0}_{1}", rowId, colId)],
                        end: pm.ControlPoints[string.Format("I_{0}_{1}", rowId + 1, colId)]
                        );
                    path.SpeedByDensity = d => SpeedByDensity(d / 2);

                    path = pm.CreatePath(
                        tag: string.Format("P_{0}_{1}_U", rowId, colId),
                        length: block_length / 2,
                        capacity: capacity,
                        start: pm.ControlPoints[string.Format("O_{0}_{1}", rowId + 1, colId)],
                        end: pm.ControlPoints[string.Format("I_{0}_{1}", rowId, colId)]
                        );
                    path.SpeedByDensity = d => SpeedByDensity(d / 2);
                }
            }

            //foreach (var path in pm.Paths.Values)
            //    path.D = string.Format("M {0} {1} L {2} {3}", path.Start.X, path.Start.Y, path.End.X, path.End.Y);

            pm.RoutingTablesFile = "routing_table_pm_example2.txt";
            //pm.OutputRoutingTables();

            //new SVG(5000, 5000, PathMover.Statics.SVGDefs, pm.SVG(x: 100, y: 100)).View();

            return pm;
        }
    }
}
