using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    public class LinearTool
    {
        public static DenseVector Rotate(DenseVector point, DenseVector centre, double theta)
        {
            return DenseMatrix.OfRowArrays(new double[][] {
                new double[] {Math.Cos(theta), - Math.Sin(theta) },
                new double[] {Math.Sin(theta), Math.Cos(theta) }
            }) * DenseMatrix.OfColumnVectors(new DenseVector[] { point - centre }).ToColumnArrays()[0]
            + centre;
        }

        public static double TotalDistance(List<DenseVector> coords)
        {
            double ttlDist = 0;
            for (int i = 0; i < coords.Count - 1; i++) ttlDist += (coords[i + 1] - coords[i]).L2Norm();
            return ttlDist;
        }

        internal static DenseVector SlipByDistance(DenseVector point, DenseVector towards, double distance)
        {
            return SlipByRatio(point, towards, distance / (towards - point).L2Norm());
        }

        internal static DenseVector SlipByRatio(DenseVector point, DenseVector towards, double ratio)
        {
            return point + (towards - point) * ratio;
        }

        public static DenseVector SlipOnCurve(List<DenseVector> coords, ref DenseVector towards, double ratio)
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

        internal static List<DenseVector> GetCoordsInRange(List<DenseVector> coords, double startRatio, double endRatio)
        {
            var range = new List<DenseVector>();
            var distances = new List<double>();
            for (int i = 0; i < coords.Count - 1; i++)
                distances.Add((coords[i + 1] - coords[i]).L2Norm());
            var total = distances.Sum();
            var cum = 0d;
            var lbDist = startRatio < endRatio ? total * startRatio : total * endRatio;
            var ubDist = startRatio < endRatio ? total * endRatio : total * startRatio;
            for (int i = 0; i < distances.Count; i++)
            {
                cum += distances[i];

                if (cum >= lbDist)
                {
                    if (range.Count == 0) range.Add(coords[i + 1] - (coords[i + 1] - coords[i]) / distances[i] * (cum - lbDist));
                    else range.Add(coords[i]);
                }
                if (cum >= ubDist)
                {
                    range.Add(coords[i + 1] - (coords[i + 1] - coords[i]) / distances[i] * (cum - ubDist));
                    break;
                }
            }
            if (startRatio > endRatio) range.Reverse();
            return range;
        }
    }
}
