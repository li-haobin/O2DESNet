using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    public class DrawingParams
    {
        // absolute image size
        public int Width { get; internal set; } // pixels
        public int Height { get; internal set; } // pixels
        public int Margin { get; set; } // pixels

        // Coordinate Ranges
        private double _minX, _maxX, _minY, _maxY;

        // element size
        public int PathThickness { get; set; } = 1; // pixels
        public int ControlPointThickness { get; set; } = 2; // pixels
        public double ArrowSize { get; set; } = 6; // relative value
        public double ArrowAngle { get; set; } = Math.PI / 5; // radius
        public double ControlPointSize { get; set; } = 6; // relative value

        // colors
        public Color PathColor { get; set; } = Color.DarkSlateGray;
        public Color ControlPointColor { get; set; } = Color.DarkRed;

        public DrawingParams(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Point GetPoint(IEnumerable<double> coord)
        {
            return new Point(
                (int)Math.Round(Margin + (Width - Margin * 2) * (coord.ElementAt(0) - _minX) / (_maxX - _minX), 0),
                (int)Math.Round(Margin + (Height - Margin * 2) * (coord.ElementAt(1) - _minY) / (_maxY - _minY), 0)
                );
        }

        public void Init(IEnumerable<double[]> coords)
        {
            _maxX = coords.Max(c => c[0]);
            _minX = coords.Min(c => c[0]);
            _maxY = coords.Max(c => c[1]);
            _minY = coords.Min(c => c[1]);
            Height = Math.Min(Height, (int)Math.Round(Width / (_maxX - _minX) * (_maxY - _minY), 0));
            Width = Math.Min(Width, (int)Math.Round(Height / (_maxY - _minY) * (_maxX - _minX), 0));
            Margin = Math.Max(Margin, (int)Math.Round(Math.Max(Height * 0.02, Width * 0.02)));
        }

    }
}
