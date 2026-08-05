using System;

namespace O2DESNet
{
    /// <summary>
    /// Immutable 2D spatial transform with position, rotation, and flip state.
    /// Supports composition via * and decomposition via / operators.
    /// </summary>
    public readonly record struct Pointer(
        double X = 0,
        double Y = 0,
        double Angle = 0,
        bool Flipped = false)
    {
        /// <summary>
        /// Super-position of two pointers (inner * outer).
        /// Applies inner's local transform on top of outer's world transform.
        /// </summary>
        public static Pointer operator *(Pointer inner, Pointer outer)
        {
            var radians = outer.Angle / 180 * Math.PI;
            return new Pointer(
                X: inner.X * Math.Cos(radians) - inner.Y * Math.Sin(radians) + outer.X,
                Y: inner.Y * Math.Cos(radians) + inner.X * Math.Sin(radians) + outer.Y,
                Angle: (outer.Angle + inner.Angle) % 360,
                Flipped: outer.Flipped ^ inner.Flipped
            );
        }

        /// <summary>
        /// Get the inner pointer by removing outer's transform from the product.
        /// </summary>
        public static Pointer operator /(Pointer product, Pointer outer)
        {
            return product * new Pointer(X: -outer.X, Y: -outer.Y)
                * new Pointer(Angle: -outer.Angle, Flipped: outer.Flipped);
        }
    }
}
