using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Distributions
{
    public class PolarSampling
    {
        #region Reference: Li et al. (2015), "The Steerable Stochastic Search on the Strength of Hyper-Spherical Coordinates"
        internal static double[] Uniform(int dimension, Random rs)
        {
            #region Theorem 1
            return Vector.Normalize(
                Enumerable.Range(0, (int)Math.Ceiling(0.5 * dimension))
                .SelectMany(i => StandardNormal(rs)).Take(dimension)
                .ToArray());
            #endregion
        }

        internal static double[] Oriented(double[] direction, double sigma, Random rs)
        {
            if (Vector.Norm(direction) == 0) direction = Uniform(direction.Length, rs);
            if (sigma == 0) return direction;
            int dimension = direction.Count();
            var indices = Enumerable.Range(0, dimension);

            #region Algorithm 1
            /// sample the angle deviated from the OrientingVector, formula (9)
            double? theta = null;
            while (theta == null)
            {
                theta = Math.Abs(StandardNormal(rs)[0] * sigma);
                if (theta > Math.PI || rs.NextDouble() > Math.Pow(Math.Sin((double)theta), dimension - 2)) theta = null;
            }

            /// sample points oriented by the "North Pole"
            double sinTheta = Math.Sin(theta.Value), cosTheta = Math.Cos(theta.Value);
            var sampleVector = Uniform(dimension - 1, rs).Select(v => v * sinTheta).Concat(new double[] { cosTheta }).ToArray();
            #endregion

            #region Definition 4
            /// construct the "North Pole"
            var northPole = Enumerable.Repeat(0.0, dimension).ToArray();
            northPole[dimension - 1] = 1;

            /// find the normalized middle line between "North Pole" and given orienting vector
            var middle = Vector.Normalize(Vector.Divide(Vector.Add(Vector.Normalize(direction), northPole), 2));

            /// reflect the sampled point according to the middle line
            /// and rescale it to the length of orienting vector
            return Vector.Multiply(Vector.Subtract(Vector.Multiply(middle, Vector.DotProduct(sampleVector, middle) * 2), sampleVector), Vector.Norm(direction)).ToArray();
            #endregion
        }
        #endregion

        private static double[] StandardNormal(Random rs)
        {
            // Box-Muller transform:
            double u1 = rs.NextDouble(), u2 = rs.NextDouble();
            double r = Math.Sqrt(-2.0 * Math.Log(u1));
            double x1 = Math.Sin(2.0 * Math.PI * u2), x2 = Math.Cos(2.0 * Math.PI * u2);
            return new double[] { r * x1, r * x2 };
        }

    }
}
