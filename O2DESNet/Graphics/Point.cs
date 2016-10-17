using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Point
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public Point(double x, double y) { X = x; Y = y; }

        /// <summary>
        /// Euclidean distance to the given point
        /// </summary>
        public double Distance(Point point) { return (point - this).L2Norm(); }
        public double L2Norm() { return Math.Sqrt(X * X + Y * Y); }
        public double Degree() { return Math.Atan2(Y, X) / Math.PI * 180; }
        
        public static Point operator +(Point pt1, Point pt2) { return new Point(pt1.X + pt2.X, pt1.Y + pt2.Y); }
        public static Point operator -(Point pt1, Point pt2) { return new Point(pt1.X - pt2.X, pt1.Y - pt2.Y); }
        public static Point operator -(Point pt) { return new Point(-pt.X, -pt.Y); }
        public static Point operator *(Point pt, double scale) { return new Point(pt.X * scale, pt.Y * scale); }
        public static Point operator /(Point pt, double scale) { return new Point(pt.X / scale, pt.Y / scale); }

        /// <summary>
        /// Get coordinates and directions of a list of interest points on a given curve by given their ratios of the total curve length
        /// </summary>
        /// <param name="coords">Points that form the curve</param>
        /// <param name="ratios">The proportions of the interests point on the curve</param>
        /// <returns>List of Tuples, in each Item1 is coordinate of the interest point, and Item2 is the direction</returns>
        public static List<Tuple<Point, Point>> SlipOnCurve(List<Point> coords, List<double> ratios)
        {
            var indices = Enumerable.Range(0, ratios.Count).OrderBy(i => ratios[i]).ToList();
            var distances = new List<double>();
            for (int i = 0; i < coords.Count - 1; i++) distances.Add(coords[i].Distance(coords[i + 1]));
            var total = distances.Sum();
            var results = ratios.Select(p => (Tuple<Point, Point>)null).ToList();
            double cum = distances.First();
            int k = 0, j = 0;
            while (k < ratios.Count && j < coords.Count)
            {
                
                var dist = total * ratios[indices[k]];

                if (dist <= cum)
                {
                    results[indices[k]] = new Tuple<Point, Point>(
                        coords[j + 1] - (coords[j + 1] - coords[j]) / distances[j] * (cum - dist),
                        coords[j + 1] - coords[j]);
                    k++;
                }
                else
                {
                    j++;
                    cum += distances[j];                    
                }
            }
            return results;
        }
    }
}
