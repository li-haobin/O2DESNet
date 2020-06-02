using System;

namespace O2DESNet
{
    public struct Pointer
    {
        /// <summary>
        /// Get an empty Pointer
        /// </summary>
        public static readonly Pointer Empty = new Pointer(x: 0, y: 0, angle: 0, flipped: false);

        /// <summary>
        /// Gets the x value.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the y value.
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Gets the angle value.
        /// </summary>
        public double Angle { get; }

        /// <summary>
        /// Gets a value indicating whether the angle is flipped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if flipped; otherwise, <c>false</c>.
        /// </value>
        public bool Flipped { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pointer"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="flipped">if set to <c>true</c> [flipped].</param>
        public Pointer(double x, double y, double angle, bool flipped)
        {
            X = x;
            Y = y;
            Angle = angle;
            Flipped = flipped;
        }

        /// <summary>
        /// Super position of two pointer
        /// </summary>
        public static Pointer operator *(Pointer inner, Pointer outer)
        {
            var radius = outer.Angle / 180 * Math.PI;
            return new Pointer(
                x: inner.X * Math.Cos(radius) - inner.Y * Math.Sin(radius) + outer.X,
                y: inner.Y * Math.Cos(radius) + inner.X * Math.Sin(radius) + outer.Y,
                angle: (outer.Angle + inner.Angle) % 360,
                flipped: outer.Flipped ^ inner.Flipped
            );
        }

        /// <summary>
        /// Get the inner pointer
        /// </summary>
        public static Pointer operator /(Pointer product, Pointer outer)
        {
            return product * new Pointer(-outer.X, -outer.Y, angle: 0, flipped: false)
                * new Pointer(x: 0, y: 0, -outer.Angle, outer.Flipped);
        }
    }
}