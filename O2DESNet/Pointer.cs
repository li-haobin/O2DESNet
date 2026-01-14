using System;

namespace O2DESNet;

public struct Pointer
{
    public double X { get; }
    public double Y { get; }
    public double Angle { get; }
    public bool Flipped { get; }
    public Pointer(double x = 0, double y = 0, double angle = 0, bool flipped = false)
    {
        X = x;
        Y = y;
        Angle = angle;
        Flipped = flipped;
    }

    /// <summary>
    /// Super position of two pointer
    /// </summary>
    public static Pointer operator *(Pointer inner, Pointer outter)
    {
        var radius = outter.Angle / 180 * Math.PI;
        return new Pointer(
            x: inner.X * Math.Cos(radius) - inner.Y * Math.Sin(radius) + outter.X,
            y: inner.Y * Math.Cos(radius) + inner.X * Math.Sin(radius) + outter.Y,
            angle: (outter.Angle + inner.Angle) % 360,
            flipped: outter.Flipped ^ inner.Flipped
        );
    }
    /// <summary>
    /// Get the inner pointer
    /// </summary>
    public static Pointer operator /(Pointer product, Pointer outter)
    {
        return product * new Pointer(x: -outter.X, y: -outter.Y)
            * new Pointer(angle: -outter.Angle, flipped: outter.Flipped);
    }
}