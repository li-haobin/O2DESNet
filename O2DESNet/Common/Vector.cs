using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    internal class Vector
    {
        internal static double[] Normalize(double[] vector) { var norm = Norm(vector); return vector.Select(v => v / norm).ToArray(); }
        internal static double Norm(double[] vector) { return Math.Sqrt(vector.Sum(v => v * v)); }
        internal static double[] Add(double[] vector1, double[] vector2) { return Enumerable.Range(0, vector1.Count()).Select(i => vector1[i] + vector2[i]).ToArray(); }
        internal static double[] Subtract(double[] vector1, double[] vector2) { return Enumerable.Range(0, vector1.Count()).Select(i => vector1[i] - vector2[i]).ToArray(); }
        internal static double DotProduct(double[] vector1, double[] vector2) { return Enumerable.Range(0, vector1.Count()).Sum(i => vector1[i] * vector2[i]); }
        internal static double[] Divide(double[] vector, double scalar) { return vector.Select(v => v / scalar).ToArray(); }
        internal static double[] Multiply(double[] vector, double scalar) { return vector.Select(v => v * scalar).ToArray(); }
    }
}
