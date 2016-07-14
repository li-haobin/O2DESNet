using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    internal class LinearTool
    {
        internal static DenseVector Rotate(DenseVector point, DenseVector centre, double theta)
        {
            return DenseMatrix.OfRowArrays(new double[][] {
                new double[] {Math.Cos(theta), - Math.Sin(theta) },
                new double[] {Math.Sin(theta), Math.Cos(theta) }
            }) * DenseMatrix.OfColumnVectors(new DenseVector[] { point - centre }).ToColumnArrays()[0]
            + centre;
        }

        internal static DenseVector SlipByDistance(DenseVector point, DenseVector towards, double distance)
        {
            return SlipByRatio(point, towards, distance / (towards - point).L2Norm());
        }

        internal static DenseVector SlipByRatio(DenseVector point, DenseVector towards, double ratio)
        {
            return point + (towards - point) * ratio;
        }
    }
}
