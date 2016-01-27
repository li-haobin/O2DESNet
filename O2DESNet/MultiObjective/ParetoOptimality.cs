using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.MultiObjective
{
    public class ParetoOptimality
    {
        /// <summary>
        /// Check if p1 is dominating p2
        /// </summary>
        public static bool Dominate(double[] p1, double[] p2)
        {
            if (p1.Length != p2.Length) throw new Exception("Point dimention is not consistent.");
            bool superior = false;
            for (int i = 0; i < p1.Length; i++)
            {
                if (p1[i] > p2[i]) return false;
                if (p1[i] < p2[i]) superior = true;
            }
            return superior;
        }
        internal static double[][] GetParetoSet(double[][] points) { return GetParetoSet(points, Dominate); }
        public static T[] GetParetoSet<T>(T[] points, Func<T, T, bool> dominate)
        {
            List<T> paretoSet = new List<T>();
            foreach (var point in points)
            {
                bool dominated = false;
                foreach (var pareto in paretoSet.ToList())
                {
                    if (dominate(pareto, point)) { dominated = true; break; }
                    if (dominate(point, pareto)) paretoSet.Remove(pareto);
                }
                if (!dominated) paretoSet.Add(point);
            }
            return paretoSet.ToArray();
        }
        public static double DominatedArea(double[][] points, double[] reference = null)
        {
            if (reference == null) reference = GetWorstPoint(points);
            if (reference.Length < 1) return 0;
            else if (reference.Length < 2) return reference[0] - points.Min(p => p[0]);
            double area = 0;
            var orderedPoints = GetParetoSet(FilterPointsByReference(points, reference)).OrderBy(p => p[0]).ToList();
            for (int i = 0; i < orderedPoints.Count; i++)
            {
                double width;
                if (i < orderedPoints.Count - 1) width = orderedPoints[i + 1][0] - orderedPoints[i][0];
                else width = reference[0] - orderedPoints[i][0];
                double height = reference[1] - orderedPoints[i][1];
                area += height * width;
            }
            return area;
        }
        /// <summary>
        /// The HSO Method
        /// </summary>
        public static double DominatedHyperVolume(double[][] points, double[] reference = null)
        {
            if (reference == null) reference = GetWorstPoint(points);
            int dimension = reference.Length;
            if (dimension < 3) return DominatedArea(points, reference);

            // 1. filter by reference
            // 2. identify the Pareto set 
            // 3. sort according to the value at sweeping coordinate
            var orderedPoints = GetParetoSet(FilterPointsByReference(points, reference)).OrderBy(p => p[0]).ToList();

            //calculation
            double hyperVolume = 0;
            var reducedPoints = new List<double[]>();
            var reducedReference = reference.Skip(1).ToArray();
            for (int i = 0; i < orderedPoints.Count; i++)
            {
                double depth;
                if (i < orderedPoints.Count - 1) depth = orderedPoints[i + 1][0] - orderedPoints[i][0];
                else depth = reference[0] - orderedPoints[i][0];
                var newPoint = orderedPoints[i].Skip(1).ToArray();
                reducedPoints.Add(newPoint);
                double hyperArea;
                hyperArea = DominatedHyperVolume(reducedPoints.ToArray(), reducedReference);
                hyperVolume += hyperArea * depth;
            }
            return hyperVolume;
        }

        /// <summary>
        /// Cut down volume beyond boundaries set by reference point
        /// </summary>
        private static double[][] FilterPointsByReference(double[][] points, double[] reference)
        {
            var indices = Enumerable.Range(0, reference.Length);
            return points.Where(p => indices.Count(i => p[i] > reference[i]) < 1).ToArray();
        }
        private static double[] GetWorstPoint(double[][] points)
        {
            int dim = points.Min(p => p.Length);
            var point = new List<double>();
            for (int i = 0; i < dim; i++) point.Add(points.Max(p => p[i]));
            return point.ToArray();
        }
    }
}
