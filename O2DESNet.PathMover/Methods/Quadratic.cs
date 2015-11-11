using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Methods
{
    internal class Quadratic
    {
        /// <summary>
        /// Find the root of a quadratic equation
        /// </summary>
        /// <param name="epsilon">threshold for delta to be negative, to adjust from rounding error</param>
        /// <returns></returns>
        internal static double[] Solve(double a, double b, double c, double epsilon = 0)
        {
            var delta = b * b - a * c * 4;
            if (delta < -epsilon * epsilon) return new double[] { };
            else
            {
                if (delta <= epsilon * epsilon) return new double[] { -b / (a * 2) };
                else return new List<double> { (-b - Math.Sqrt(delta)) / (a * 2), (-b + Math.Sqrt(delta)) / (a * 2) }.OrderBy(v => v).ToArray();
            }
        }
    }
}
