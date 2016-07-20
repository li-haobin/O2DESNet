using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public class Pareto
    {
        /// <summary>
        /// Check if p1 is dominating p2
        /// </summary>
        public static bool Dominate(DenseVector p1, DenseVector p2)
        {
            if (p1.Count != p2.Count) throw new Exception_InconsistentDimensions();
            bool superior = false;
            for (int i = 0; i < p1.Count; i++)
            {
                if (p1[i] > p2[i]) return false;
                if (p1[i] < p2[i]) superior = true;
            }
            return superior;
        }
        internal static DenseVector[] GetParetoSet(IEnumerable<DenseVector> points) { return GetParetoSet(points, p => p); }
        public static T[] GetParetoSet<T>(IEnumerable<T> points, Func<T, DenseVector> getValues)
        {
            List<T> paretoSet = new List<T>();
            Func<T, T, bool> dominate = (p1, p2) => Dominate(getValues(p1), getValues(p2));
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
        public static double DominatedArea(IEnumerable<DenseVector> points, DenseVector reference = null)
        {
            if (reference == null) reference = GetWorstPoint(points);
            if (reference.Count < 1) return 0;
            else if (reference.Count < 2) return reference[0] - points.Min(p => p[0]);
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
        public static double DominatedHyperVolume(IEnumerable<DenseVector> points, DenseVector reference = null)
        {
            if (reference == null) reference = GetWorstPoint(points);
            int dimension = reference.Count;
            if (dimension < 3) return DominatedArea(points, reference);

            // 1. filter by reference
            // 2. identify the Pareto set 
            // 3. sort according to the value at sweeping coordinate
            var orderedPoints = GetParetoSet(FilterPointsByReference(points, reference)).OrderBy(p => p[0]).ToList();

            //calculation
            double hyperVolume = 0;
            var reducedPoints = new List<DenseVector>();
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
        private static DenseVector[] FilterPointsByReference(IEnumerable<DenseVector> points, DenseVector reference)
        {
            var indices = Enumerable.Range(0, reference.Count);
            return points.Where(p => indices.Count(i => p[i] > reference[i]) < 1).ToArray();
        }
        private static double[] GetWorstPoint(IEnumerable<DenseVector> points)
        {
            int dim = points.Min(p => p.Count);
            var point = new List<double>();
            for (int i = 0; i < dim; i++) point.Add(points.Max(p => p[i]));
            return point.ToArray();
        }

        public static List<T> NonDominatedSort<T>(IEnumerable<T> points, Func<T,DenseVector> getValues)
        {
            var ub = GetWorstPoint(points.Select(p => getValues(p)));
            var lb = GetWorstPoint(points.Select(p => -getValues(p)));
            var from = points.ToList();
            var to = new List<T>();
            while (from.Count > 0)
            {
                var paretoSet = GetParetoSet(from, getValues);
                var crowdDistances = CrowdDistance.Calculate(paretoSet.Select(p => getValues(p)), lb, ub);
                to.AddRange(paretoSet.OrderByDescending(p => crowdDistances[getValues(p)]));
                from = from.Except(paretoSet).ToList();
            }
            return to;
        }

        /// <summary>
        /// Estimate the probability of correct selection by Monte Carlo sampling
        /// </summary>
        public static double PCS_MonteCarlo(IEnumerable<StochasticSolution> solutions, Random rs, int sampleSize)
        {
            var observedParetoSet = new HashSet<StochasticSolution>(GetParetoSet(solutions, s => s.Objectives));
            int countCS = 0;
            for (int i = 0; i < sampleSize; i++)
            {
                var trueSet = GetParetoSet(solutions, s => s.PopMeans(rs)).ToList();
                if (trueSet.Count == observedParetoSet.Count)
                {
                    bool equal = true;
                    foreach (var s in trueSet) if (!observedParetoSet.Contains(s)) { equal = false; break; }
                    if (equal) countCS++;
                }
            }
            return 1.0 * countCS / sampleSize;
        }
    }

}
