using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Methods
{
    internal class Quadratic
    {
        internal static double[] Solve(double a, double b, double c)
        {
            var delta = b * b - a * c * 4;
            if (delta < 0) return new double[] { };
            else if (delta == 0) return new double[] { -b / (a * 2) };
            else return new double[] { (-b - Math.Sqrt(delta)) / (a * 2), (-b + Math.Sqrt(delta)) / (a * 2) };
        }
    }
}
