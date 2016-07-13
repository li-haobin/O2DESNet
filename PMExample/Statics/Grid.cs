using O2DESNet.PathMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMExample
{
    public class Grid : PMStatics
    {
        /// <summary>
        /// All control points that connect paths in the grid, with dimension [row, col]
        /// </summary>
        public ControlPoint[,] ConnectingPoints { get; private set; }
        /// <summary>
        /// All path in rows, with dimension [row][col]
        /// </summary>
        public Path[][] RowPaths { get; private set; }
        /// <summary>
        /// All path in columns, with dimension [col][row]
        /// </summary>
        public Path[][] ColPaths { get; private set; }

        public Grid(double[] colSpaces, double[] rowSpaces, double fullSpeed) : base()
        {
            ConnectingPoints = new ControlPoint[rowSpaces.Length + 1, colSpaces.Length + 1];
            RowPaths = Enumerable.Range(0, rowSpaces.Length + 1).Select(i => Enumerable.Range(0, colSpaces.Length).Select(j => CreatePath(colSpaces[j], fullSpeed)).ToArray()).ToArray();
            ColPaths = Enumerable.Range(0, colSpaces.Length + 1).Select(j => Enumerable.Range(0, rowSpaces.Length).Select(i => CreatePath(rowSpaces[i], fullSpeed)).ToArray()).ToArray();
            for (int i = 0; i < rowSpaces.Length + 1; i++)
            {
                ConnectingPoints[i, 0] = CreateControlPoint(RowPaths[i][0], 0); // starting CP for the row
                for (int j = 0; j < colSpaces.Length - 1; j++)
                {
                    Connect(RowPaths[i][j], RowPaths[i][j + 1]);
                    ConnectingPoints[i, j + 1] = RowPaths[i][j + 1].ControlPoints.First(); // connecting CP in the row
                }
                ConnectingPoints[i, colSpaces.Length] = CreateControlPoint(RowPaths[i][colSpaces.Length - 1], RowPaths[i][colSpaces.Length - 1].Length); // ending CP for the row
            }
            for (int j = 0; j < colSpaces.Length + 1; j++)
            {
                for (int i = 0; i < rowSpaces.Length; i++)
                {
                    // assign starting & ending CP for each column path
                    Connect(ColPaths[j][i], 0, ConnectingPoints[i, j]);
                    Connect(ColPaths[j][i], ColPaths[j][i].Length, ConnectingPoints[i + 1, j]);
                }
            }

            #region Generate Coordinates
            double x, y;
            y = 0;
            for (int i = 0; i < rowSpaces.Length + 1; i++)
            {
                if (i > 0) y += rowSpaces[i - 1];
                x = 0;
                for (int j = 0; j < colSpaces.Length; j++)
                {
                    PathCoordinates.Add(RowPaths[i][j], new double[] { x, y, x + colSpaces[j], y });
                    x += colSpaces[j];
                }
            }
            x = 0;
            for (int j = 0; j < colSpaces.Length + 1; j++)
            {
                if (j > 0) x += colSpaces[j - 1];
                y = 0;
                for (int i = 0; i < rowSpaces.Length; i++)
                {
                    PathCoordinates.Add(ColPaths[j][i], new double[] { x, y, x, y + rowSpaces[i] });
                    y += rowSpaces[i];
                }
            }
            #endregion
        }
    }
}
