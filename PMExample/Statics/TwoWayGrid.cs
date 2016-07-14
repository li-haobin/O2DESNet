using MathNet.Numerics.LinearAlgebra.Double;
using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample
{
    public class TwoWayGrid : PMScenario
    {
        /// <summary>
        /// All control points that connect paths in the grid, with dimension [row, col, out/in]
        /// </summary>
        public ControlPoint[,,] ConnectingPoints { get; private set; }

        /// <summary>
        /// All path in rows, with dimension [row][col][L/R], R=0, L=1
        /// </summary>
        public Path[][][] RowPaths { get; private set; }

        /// <summary>
        /// All path in columns, with dimension [col][row][D/U], D=0, U=1
        /// </summary>
        public Path[][][] ColPaths { get; private set; }
        /// <summary>
        /// All virtual paths at junctions [row][col]
        /// </summary>
        public Path[][] JunctionPaths { get; private set; }

        public TwoWayGrid(double[] colSpaces, double[] rowSpaces, double fullSpeed) : base()
        {
            ConnectingPoints = new ControlPoint[rowSpaces.Length + 1, colSpaces.Length + 1, 2];
            RowPaths = Enumerable.Range(0, rowSpaces.Length + 1).Select(i => Enumerable.Range(0, colSpaces.Length).Select(j => new Path[] {
                CreatePath(colSpaces[j], fullSpeed, Direction.Forward),
                CreatePath(colSpaces[j], fullSpeed, Direction.Backward)
            }).ToArray()).ToArray();
            ColPaths = Enumerable.Range(0, colSpaces.Length + 1).Select(j => Enumerable.Range(0, rowSpaces.Length).Select(i => new Path[] {
                CreatePath(colSpaces[i], fullSpeed, Direction.Forward),
                CreatePath(colSpaces[i], fullSpeed, Direction.Backward)
            }).ToArray()).ToArray();
            JunctionPaths = Enumerable.Range(0, rowSpaces.Length + 1).Select(i => Enumerable.Range(0, colSpaces.Length + 1).Select(j => CreatePath(1E-6, fullSpeed, Direction.Forward)).ToArray()).ToArray();

            for (int i = 0; i < rowSpaces.Length + 1; i++)
            {
                // starting CPs for the row
                ConnectingPoints[i, 0, 0] = CreateControlPoint(RowPaths[i][0][0], 0); 
                ConnectingPoints[i, 0, 1] = CreateControlPoint(RowPaths[i][0][1], 0);
                for (int j = 0; j < colSpaces.Length - 1; j++)
                {
                    // connecting CP in the row
                    Connect(RowPaths[i][j][0], RowPaths[i][j + 1][1]);
                    Connect(RowPaths[i][j][1], RowPaths[i][j + 1][0]);
                    ConnectingPoints[i, j + 1, 0] = RowPaths[i][j + 1][0].ControlPoints.First();
                    ConnectingPoints[i, j + 1, 1] = RowPaths[i][j + 1][1].ControlPoints.First();
                }
                // ending CP for the row
                ConnectingPoints[i, colSpaces.Length, 0] = CreateControlPoint(
                    RowPaths[i][colSpaces.Length - 1][1], RowPaths[i][colSpaces.Length - 1][1].Length); 
                ConnectingPoints[i, colSpaces.Length, 1] = CreateControlPoint(
                    RowPaths[i][colSpaces.Length - 1][0], RowPaths[i][colSpaces.Length - 1][0].Length);
            }
            for (int j = 0; j < colSpaces.Length + 1; j++)
            {
                for (int i = 0; i < rowSpaces.Length; i++)
                {
                    // assign starting & ending CP for each column path
                    Connect(ColPaths[j][i][0], 0, ConnectingPoints[i, j, 0]);
                    Connect(ColPaths[j][i][1], 0, ConnectingPoints[i, j, 1]);
                    Connect(ColPaths[j][i][0], ColPaths[j][i][0].Length, ConnectingPoints[i + 1, j, 1]);
                    Connect(ColPaths[j][i][1], ColPaths[j][i][1].Length, ConnectingPoints[i + 1, j, 0]);
                }
            }
            for (int i = 0; i < rowSpaces.Length + 1; i++)
            {
                for (int j = 0; j < colSpaces.Length + 1; j++)
                {
                    Connect(JunctionPaths[i][j], 0, ConnectingPoints[i, j, 1]);
                    Connect(JunctionPaths[i][j], JunctionPaths[i][j].Length, ConnectingPoints[i, j, 0]);
                }
            }

            #region Generate Coordinates
            var cpCoords = new Dictionary<ControlPoint, DenseVector>();
            double x, y;
            y = 0;
            for (int i = 0; i < rowSpaces.Length + 1; i++)
            {
                if (i > 0) y += rowSpaces[i - 1];
                x = 0;
                for (int j = 0; j < colSpaces.Length + 1; j++)
                {
                    if (j > 0) x += colSpaces[j - 1];
                    cpCoords.Add(ConnectingPoints[i, j, 0], new double[] { x, y });
                    cpCoords.Add(ConnectingPoints[i, j, 1], new double[] { x + 10, y + 10 });
                }
            }
            foreach(var path in Paths)
            {
                path.Coordinates.Add(cpCoords[path.ControlPoints.First()]);
                path.Coordinates.Add(cpCoords[path.ControlPoints.Last()]);
            }
            #endregion

        }
    }
}
