using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public class ConvexSet
    {
        public int Dimension { get; private set; }
        internal HashSet<Constraint> Constraints { get; private set; }

        /// <param name="dimension">dimension of the space</param>
        /// <param name="globalLb">inclusive global lowerbounds for all variables</param>
        /// <param name="globalUb">inclusive global upperbounds for all variables</param>
        public ConvexSet(int dimension, double globalLb = double.NegativeInfinity, double globalUb = double.PositiveInfinity, 
            DenseVector lowerbounds = null, DenseVector upperbounds = null, IEnumerable<Constraint> constraints = null)
        {
            Dimension = dimension;
            Constraints = new HashSet<Constraint>();
            var identityRowArray = DenseMatrix.CreateIdentity(dimension).ToRowArrays();
            if (globalLb > double.NegativeInfinity) Add(identityRowArray.Select(coeffs => new ConstraintGE(coeffs, globalLb)));
            if (globalUb < double.PositiveInfinity) Add(identityRowArray.Select(coeffs => new ConstraintLE(coeffs, globalUb)));
            if (lowerbounds != null) for (int i = 0; i < dimension; i++) Add(new ConstraintGE(identityRowArray[i], lowerbounds[i]));
            if (upperbounds != null) for (int i = 0; i < dimension; i++) Add(new ConstraintLE(identityRowArray[i], upperbounds[i]));
            if (constraints != null) foreach (var constraint in constraints) Constraints.Add(constraint);
        }

        public void Add(Constraint constraint) { Constraints.Add(constraint); }
        public void Add(IEnumerable<Constraint> constraints) { foreach (var constraint in constraints) Add(constraint); }
        
        /// <summary>
        /// Chech if the given point is inside the convex set
        /// </summary>
        internal bool Contains(DenseVector point)
        {
            if (point.Count != Dimension) return false;
            foreach (var constraint in Constraints) if (!constraint.FeasibleFor(point)) return false;
            return true;
        }

        internal double Distance(DenseVector point, DenseVector direction)
        {
            double distance = double.PositiveInfinity;
            foreach (var constraint in Constraints) distance = Math.Min(distance, constraint.SlackDistance(point, direction));
            return distance;
        }
    }
}
