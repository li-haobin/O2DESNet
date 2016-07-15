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

        internal static DenseVector SlipOnCurve(List<DenseVector> coords, ref DenseVector towards, double ratio)
        {
            var distances = new List<double>();
            for (int i = 0; i < coords.Count - 1; i++)
                distances.Add((coords[i + 1] - coords[i]).L2Norm());
            var total = distances.Sum();
            var cum = 0d;
            var dist = total * ratio;
            for (int i = 0; i < distances.Count; i++)
            {
                cum += distances[i];
                if (dist <= cum)
                {
                    var  p = coords[i + 1] - (coords[i + 1] - coords[i]) / distances[i] * (cum - dist);
                    towards = p + (coords[i + 1] - coords[i]);
                    return p;
                }
            }
            return null;
        }

        internal static List<DenseVector> GetCoordsInRange(List<DenseVector> coords, double lbRatio, double ubRatio)
        {
            var range = new List<DenseVector>();
            var distances = new List<double>();
            for (int i = 0; i < coords.Count - 1; i++)
                distances.Add((coords[i + 1] - coords[i]).L2Norm());
            var total = distances.Sum();
            var cum = 0d;
            var lbDist = total * lbRatio;
            var ubDist = total * ubRatio;
            for (int i = 0; i < distances.Count; i++)
            {
                cum += distances[i];
                if (cum > ubDist) return range;
                if (cum >= lbDist) range.Add(coords[i]);                
            }
            return null;
        }
    }
}
