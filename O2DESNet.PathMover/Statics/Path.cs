using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace O2DESNet.PathMover
{
    public class Path
    {
        public PMScenario Statics { get; private set; }
        public int Id { get; private set; }

        public double Length { get; set; }
        public Direction Direction { get; private set; }
        public double FullSpeed { get; private set; }
        public List<ControlPoint> ControlPoints { get; private set; }
        public List<DenseVector> Coordinates { get; private set; }
        public bool Crab { get; set; } = false;

        internal Path(PMScenario scenario, double length, double fullSpeed, Direction direction)
        {
            Statics = scenario;
            Id = Statics.Paths.Count;
            Length = length;
            FullSpeed = fullSpeed;
            Direction = direction;
            ControlPoints = new List<ControlPoint>();
            Coordinates = new List<DenseVector>();
        }

        internal void Add(ControlPoint controlPoint, double position)
        {
            controlPoint.Positions.Add(this, position);
            ControlPoints.Add(controlPoint);
            if (controlPoint.Positions[this] < 0 || controlPoint.Positions[this] > Length)
                throw new Exception("Control point must be positioned within the range of path length.");
            ControlPoints.Sort((t0, t1) => t0.Positions[this].CompareTo(t1.Positions[this]));
        }
        public double GetDistance(ControlPoint from, ControlPoint to)
        {
            if (from.Positions.ContainsKey(this) && to.Positions.ContainsKey(this))
            {
                double distance = to.Positions[this] - from.Positions[this];
                if (Direction == Direction.Backward) distance = -distance;
                else if (Direction == Direction.TwoWay) distance = Math.Abs(distance);
                if (distance >= 0) return distance;
            }
            return double.PositiveInfinity;
        }

        public override string ToString()
        {
            return string.Format("PATH{0}", Id);
        }

        public virtual void Draw(Graphics g, DrawingParams dParams, Pen pen, double start, double end)
        {
            g.DrawLines(pen, LinearTool.GetCoordsInRange(Coordinates, start, end).Select(c => dParams.GetPoint(c)).ToArray());
        }
    }

    public enum Direction { Forward, Backward, TwoWay }
}
