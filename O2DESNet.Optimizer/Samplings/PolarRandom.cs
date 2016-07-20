using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Samplings
{
    public class PolarRandom
    {
        #region Reference: Li et al. (2015), "The Steerable Stochastic Search on the Strength of Hyper-Spherical Coordinates"
        public static DenseVector Uniform(int dimension, Random rs)
        {
            #region Theorem 1
            DenseVector vector = Enumerable.Range(0, dimension).Select(i => Normal.Sample(rs, 0, 1)).ToArray();
            return vector.Normalize(2).ToArray();
            #endregion
        }

        public static DenseVector Oriented(DenseVector direction, double sigma, Random rs)
        {
            if (sigma > 100) return Uniform(direction.Count, rs);
            var norm = direction.L2Norm();
            if (norm == 0) direction = Uniform(direction.Count, rs);
            if (sigma == 0) return direction;
            int dimension = direction.Count();
            var indices = Enumerable.Range(0, dimension);

            #region Algorithm 1
            /// sample the angle deviated from the OrientingVector, formula (9)
            double? theta = null;
            while (theta == null)
            {
                theta = Math.Abs(Normal.Sample(rs, 0, 1) * sigma);
                if (theta > Math.PI || rs.NextDouble() > Math.Pow(Math.Sin((double)theta), dimension - 2)) theta = null;
            }

            /// sample points oriented by the "North Pole"
            double sinTheta = Math.Sin(theta.Value), cosTheta = Math.Cos(theta.Value);
            DenseVector sampleVector = Uniform(dimension - 1, rs).Select(v => v * sinTheta).Concat(new double[] { cosTheta }).ToArray();
            #endregion

            #region Definition 4
            /// construct the "North Pole"
            DenseVector northPole = Enumerable.Repeat(0.0, dimension).ToArray();
            northPole[dimension - 1] = 1;

            /// find the normalized middle line between "North Pole" and given orienting vector
            DenseVector middle = ((direction / norm + northPole) / 2).Normalize(2).ToArray();

            /// reflect the sampled point according to the middle line
            /// and rescale it to the length of orienting vector
            return (middle * (sampleVector.DotProduct(middle) * 2) - sampleVector) * direction.L2Norm();
            #endregion
        }
        #endregion
    }
}
