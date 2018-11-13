using O2DESNet.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;

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
        public override string ToString() { return Tag ?? base.ToString(); }

        #region Drawing
        private bool _showTag = true;
        private Canvas _drawing = null;
        public TransformGroup TransformGroup { get; } = new TransformGroup();
        public Canvas Drawing { get { if (_drawing == null) UpdDrawing(); return _drawing; } }
        public bool ShowTag { get { return _showTag; } set { if (_showTag != value) { _showTag = value; UpdDrawing(); } } }
        public void UpdDrawing(DateTime? clockTime = null)
        {
            if (_drawing == null)
            {
                _drawing = new Canvas();
                TransformGroup.Children.Add(new RotateTransform(Degree));
                TransformGroup.Children.Add(new TranslateTransform(X, Y));
                _drawing.RenderTransform = TransformGroup;
            }
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
                FontSize = 4,
                Margin = new Thickness(-10, 4, 0, 0),
            });            
        }
        #endregion

        #region XML
        [XmlType("ControlPoint")]
        public class XML
        {
            public string Index { get; set; }
            public string Tag { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public string Degree { get; set; }
            public List<string> Router { get; set; }
            public XML() { }
            public XML(ControlPoint cp)
            {
                Index = string.Format("{0:x}", cp.Index);
                Tag = cp.Tag;
                X = cp.X;
                Y = cp.Y;
                if (cp.Degree != 0) Degree = cp.Degree.ToString();
                if (cp.RoutingTable != null && cp.PathsOut.Count > 1)
                {
                    var routingList = new Dictionary<int, List<int>>();
                    foreach (var i in cp.RoutingTable)
                        if (i.Value != null && i.Key != cp)
                        {
                            var key = i.Value.Index;
                            var value = i.Key.Index;
                            if (!routingList.ContainsKey(key)) routingList.Add(key, new List<int>());
                            if (key != value) routingList[key].Add(value);
                        }
                    Router = routingList.Keys.Select(k => string.Format("{0:x}@{1}", k, routingList[k].OrderBy(v => v)
                        .Select(v => string.Format("{0:x}", v))
                        .Aggregate("", (s1, s2) => s1.Length == 0 ? s2 : string.Format("{0}|{1}", s1, s2))))
                        .OrderByDescending(str => str.Length).ToList();
                    Router.RemoveAt(0); // remove the longest routing string as it is no more than the complement
                    Router = Router.OrderBy(str => Convert.ToInt32(str.Split('@')[0], 16)).ToList();
                }
            }
            public ControlPoint Restore()
            {
                var cp = new ControlPoint
                {
                    Index = Convert.ToInt32(Index, 16),
                    Tag = Tag,
                    X = X,
                    Y = Y,
                    Degree = Convert.ToDouble(Degree),
                    RoutingTable = new Dictionary<ControlPoint, ControlPoint>(),
                };
                if (Degree != null) cp.Degree = Convert.ToDouble(Degree);
                return cp;
            }
        }
        #endregion
    }
}
