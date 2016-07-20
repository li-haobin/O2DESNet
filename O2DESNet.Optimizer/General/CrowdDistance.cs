using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public class CrowdDistance
    {
        public static Dictionary<DenseVector, double> Calculate(IEnumerable<DenseVector> points, DenseVector lowerBounds = null, DenseVector upperBounds = null)
        {
            points = points.Distinct();
            if (points.Count() == 0) throw new Exception_EmptySet();
            int dimension = points.First().Count;
            var distances = new Dictionary<DenseVector, double>();
            foreach(var point in points)
            {
                if (point.Count != dimension) throw new Exception_InconsistentDimensions();
                distances.Add(point, 0);
            }
            for (int i = 0; i < dimension; i++)
            {
                var span = Math.Max(points.Max(p => p[i]), upperBounds == null ? double.NegativeInfinity : upperBounds[i])
                    - Math.Min(points.Min(p => p[i]), lowerBounds == null ? double.PositiveInfinity : lowerBounds[i]);
                var orderedSet = points.OrderBy(p => p[i]).ToList();
                distances[orderedSet.First()] = double.PositiveInfinity;
                distances[orderedSet.Last()] = double.PositiveInfinity;
                for (int j = 1; j < orderedSet.Count - 1; j++)
                    distances[orderedSet[j]] += (orderedSet[j + 1][i] - orderedSet[j - 1][i]) / span;
            }
            return distances;
        }
    }
}
