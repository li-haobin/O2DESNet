using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace O2DESNet.Traffic
{
    public class ControlPoint : IDrawable
    {
        public int Index { get; internal set; }
        public string Tag { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Degree { get; set; } = 0;
        //public PathMover.Statics PathMover { get; internal set; }
        //internal Statics() { }

        public List<Path.Statics> PathsIn { get; private set; } = new List<Path.Statics>();
        public List<Path.Statics> PathsOut { get; private set; } = new List<Path.Statics>();
        public Dictionary<ControlPoint, ControlPoint> RoutingTable { get; internal set; }        
        public Path.Statics PathTo(ControlPoint target)
        {
            if (Equals(target)) return null;
            return PathsOut.Where(p => p.End.Equals(RoutingTable[target])).First();
        }

        #region Drawing
        private bool _showTag = true;
        private Canvas _drawing = null;
        public TransformGroup TransformGroup { get; } = new TransformGroup();
        public Canvas Drawing { get { if (_drawing == null) UpdDrawing(); return _drawing; } }
        public bool ShowTag { get { return _showTag; } set { if (_showTag != value) { _showTag = value; UpdDrawing(); } } }
        public void UpdDrawing(DateTime? clockTime = null)
        {
            if (_drawing == null) _drawing = new Canvas();
            else _drawing.Children.Clear();
            _drawing.Children.Add(new System.Windows.Shapes.Path // Cross
            {
                Stroke = Brushes.DarkRed,
                StrokeThickness = 1,
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection(new PathFigure[] {
                        new PathFigure(new Point(-4, -4), new LineSegment[]{
                            new LineSegment(new Point(4, 4), true),
                        }, false),
                        new PathFigure(new Point(-4, 4), new LineSegment[]{
                            new LineSegment(new Point(4, -4), true),
                        }, false),
                    })
                },
            });
            if (ShowTag) _drawing.Children.Add(new TextBlock
            {
                Text = Tag ?? ToString(),
                FontSize = 10,
                Margin = new Thickness(-10, 4, 0, 0),
            });
            TransformGroup.Children.Add(new RotateTransform(Degree));
            TransformGroup.Children.Add(new TranslateTransform(X, Y));
            _drawing.RenderTransform = TransformGroup;
        }
        #endregion
    }
}
