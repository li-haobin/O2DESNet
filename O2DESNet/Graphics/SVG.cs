using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class SVG
    {
        public static double Scale { get; set; } = 5; // pixels per meter by default

        public Point Size { get; set; } = new Point(100, 100);
        public Point Reference { get; set; } = new Point(150, 150);        
        public string Source { get { return string.Format("<svg width=\"{0}\" height=\"{1}\" xmlns=\"http://www.w3.org/2000/svg\">{2}{3}</svg>", Size.X, Size.Y, Head, Body); } }
        public void ToFile(string name) { using (var sw = new StreamWriter(name + ".svg")) sw.WriteLine(Source); }
        public void View()
        {
            string name = "view"; ToFile(name);
            Process.Start(string.Format("{0}.svg", name));
        }

        public string Head { get; set; } = "";
        public string Body { get; set; } = "";

        public SVG() { }
        public SVG(Path.Statics path)
        {
            var coords = path.Coords;
            if (coords == null || coords.Count < 2) coords = new List<Point> { new Point(0, 0), new Point((float)path.Length, 0) };

            Point min = new Point(coords.Min(pt => pt.X), coords.Min(pt => pt.Y));
            Point max = new Point(coords.Max(pt => pt.X), coords.Max(pt => pt.Y));
            Point margin = new Point(25, 25);
            Size = (max - min) * Scale + margin * 2;
            Reference = (coords.First() - min) * Scale + margin;

            coords = coords.Select(pt => (pt - min) * Scale + margin).ToList(); // transform coords for proper view
            // draw the path
            Body += GetDashedLine(path: coords, color: "black", dasharray: new double[] { 3, 3 }, width: 1.0);
            var marks = Point.SlipOnCurve(coords, new List<double> { 1.0 / 3, 2.0 / 3 }.Concat(path.ControlPoints.Select(cp => cp.Positions[path] / path.Length)).ToList());
            // draw the arrow
            Head += GetDefinition_ArrowMark(id: "arrow", color: "black");
            if (path.Direction != Path.Direction.Backward) Body += GetMark(position: marks[0].Item1, direction: marks[0].Item2, id: "arrow");
            if (path.Direction != Path.Direction.Forward) Body += GetMark(position: marks[1].Item1, direction: -marks[1].Item2, id: "arrow");
            // draw the mark
            var tag = string.Format("PATH{0}", path.Index);
            Point position, direction;
            if (path.Direction != Path.Direction.Backward) { position = marks[0].Item1; direction = marks[0].Item2; }
            else { position = marks[1].Item1; direction = marks[1].Item2; }
            Body += GetDefinition_TextMark_Bottom(id: tag, text: tag, color: "black") + GetMark(position, direction, tag);

            // draw the control points
            Head += GetDefinition_CrossMark(id: "cross", color: "darkred");
            for (int i = 2; i < marks.Count; i++) Body += GetControlPoint(path.ControlPoints[i - 2], position: marks[i].Item1, direction: marks[i].Item2, markId: "cross");
        }

        protected static string GetControlPoint(ControlPoint.Statics controlPoint, Point position, Point direction, string markId)
        {
            var tag = string.Format("CP{0}", controlPoint.Index);
            return GetMark(position, direction, id: markId) + GetDefinition_TextMark_TopRight(id: tag, text: tag, color: "darkred") + GetMark(position, direction, tag);
        }

        protected static string GetDashedLine(IEnumerable<Point> path, string color, IEnumerable<double> dasharray, double width)
        {
            string str = string.Format("<path d=\"M {0} {1} ", path.First().X, path.First().Y);
            for (int i = 1; i < path.Count(); i++) str += string.Format("L {0} {1}", path.ElementAt(i).X, path.ElementAt(i).Y);
            str += string.Format( "\" fill=\"none\" stroke=\"{0}\" stroke-width=\"{1}\" stroke-dasharray=\"", color, width);
            foreach (var v in dasharray) str += string.Format("{0},", v);
            str = str.Remove(str.Length - 1);
            str += "\" />";
            return str;
        }
        protected static string GetDefinition_ArrowMark(string id, string color, double width = 10, double height = 8)
        {
            return string.Format("<defs><marker id=\"{0}\" markerWidth=\"{1}\" markerHeight=\"{2}\" refX=\"{1}\" refY=\"{3}\" orient=\"auto\" markerUnits=\"strokeWidth\"><path d=\"M 0 0 L {1} {3} L 0 {2}\" fill=\"none\" stroke=\"{4}\" stroke-dasharray=\"0\"/></marker></defs>", id, width, height, height / 2, color);
        }
        protected static string GetDefinition_CrossMark(string id, string color, double width = 8)
        {
            return string.Format("<defs><marker id=\"{0}\" markerWidth=\"{1}\" markerHeight=\"{1}\" refX=\"{2}\" refY=\"{2}\" orient=\"auto\" markerUnits=\"strokeWidth\"><path d=\"M 0 0 L {1} {1} M 0 {1} L {1} 0\" fill=\"none\" stroke=\"{3}\" stroke-dasharray=\"0\"/></marker></defs>", id, width, width / 2, color);
        }
        #region Definition_TextMarks
        protected static string GetDefinition_TextMark_Bottom(string id, string text, string color, string fontFamily = "Verdana", double fontSize = 9, double padding = 6, double width = 30)
        {
            return string.Format("<defs><marker id=\"{0}\" markerWidth=\"{1}\" markerHeight=\"{2}\" refX=\"{3}\" refY=\"0\" orient=\"auto\" markerUnits=\"strokeWidth\"><text text-anchor=\"middle\" x=\"{3}\" y=\"{2}\" font-family=\"{4}\" font-size=\"{5}\" fill=\"{6}\">{7}</text></marker></defs>", id, width, fontSize + padding, width / 2, fontFamily, fontSize, color, text);
        }
        protected static string GetDefinition_TextMark_Left(string id, string text, string color, string fontFamily = "Verdana", double fontSize = 9, double padding = 6, double width = 30)
        {
            return string.Format("<defs><marker id=\"{0}\" markerWidth=\"{1}\" markerHeight=\"{2}\" refX=\"{1}\" refY=\"{3}\" orient=\"auto\" markerUnits=\"strokeWidth\"><text text-anchor=\"end\" x=\"{8}\" y=\"{5}\" font-family=\"{4}\" font-size=\"{5}\" fill=\"{6}\">{7}</text></marker></defs>", id, width, fontSize, fontSize / 2, fontFamily, fontSize, color, text, width - padding);
        }
        protected static string GetDefinition_TextMark_Right(string id, string text, string color, string fontFamily = "Verdana", double fontSize = 9, double padding = 6, double width = 30)
        {
            return string.Format("<defs><marker id=\"{0}\" markerWidth=\"{1}\" markerHeight=\"{2}\" refX=\"0\" refY=\"{3}\" orient=\"auto\" markerUnits=\"strokeWidth\"><text text-anchor=\"start\" x=\"{8}\" y=\"{5}\" font-family=\"{4}\" font-size=\"{5}\" fill=\"{6}\">{7}</text></marker></defs>", id, width, fontSize, fontSize / 2, fontFamily, fontSize, color, text, padding);
        }
        protected static string GetDefinition_TextMark_BottomLeft(string id, string text, string color, string fontFamily = "Verdana", double fontSize = 9, double padding = 6, double width = 30)
        {
            return string.Format("<defs><marker id=\"{0}\" markerWidth=\"{1}\" markerHeight=\"{2}\" refX=\"{1}\" refY=\"0\" orient=\"auto\" markerUnits=\"strokeWidth\"><text text-anchor=\"end\" x=\"{8}\" y=\"{2}\" font-family=\"{4}\" font-size=\"{5}\" fill=\"{6}\">{7}</text></marker></defs>", id, width, fontSize, width / 2, fontFamily, fontSize, color, text, width - padding);
        }        
        protected static string GetDefinition_TextMark_BottomRight(string id, string text, string color, string fontFamily = "Verdana", double fontSize = 9, double padding = 6, double width = 30)
        {
            return string.Format("<defs><marker id=\"{0}\" markerWidth=\"{1}\" markerHeight=\"{2}\" refX=\"0\" refY=\"0\" orient=\"auto\" markerUnits=\"strokeWidth\"><text text-anchor=\"start\" x=\"{8}\" y=\"{2}\" font-family=\"{4}\" font-size=\"{5}\" fill=\"{6}\">{7}</text></marker></defs>", id, width, fontSize + padding, width / 2, fontFamily, fontSize, color, text, padding);
        }
        protected static string GetDefinition_TextMark_Top(string id, string text, string color, string fontFamily = "Verdana", double fontSize = 9, double padding = 6, double width = 30)
        {
            return string.Format("<defs><marker id=\"{0}\" markerWidth=\"{1}\" markerHeight=\"{2}\" refX=\"{3}\" refY=\"{2}\" orient=\"auto\" markerUnits=\"strokeWidth\"><text text-anchor=\"middle\" x=\"{3}\" y=\"{5}\" font-family=\"{4}\" font-size=\"{5}\" fill=\"{6}\">{7}</text></marker></defs>", id, width, fontSize + padding, width / 2, fontFamily, fontSize, color, text);
        }
        protected static string GetDefinition_TextMark_TopLeft(string id, string text, string color, string fontFamily = "Verdana", double fontSize = 9, double padding = 6, double width = 30)
        {
            return string.Format("<defs><marker id=\"{0}\" markerWidth=\"{1}\" markerHeight=\"{2}\" refX=\"{1}\" refY=\"{2}\" orient=\"auto\" markerUnits=\"strokeWidth\"><text text-anchor=\"end\" x=\"{8}\" y=\"{5}\" font-family=\"{4}\" font-size=\"{5}\" fill=\"{6}\">{7}</text></marker></defs>", id, width, fontSize + padding, width / 2, fontFamily, fontSize, color, text, width - padding);
        }
        protected static string GetDefinition_TextMark_TopRight(string id, string text, string color, string fontFamily = "Verdana", double fontSize = 9, double padding = 6, double width = 30)
        {
            return string.Format("<defs><marker id=\"{0}\" markerWidth=\"{1}\" markerHeight=\"{2}\" refX=\"0\" refY=\"{2}\" orient=\"auto\" markerUnits=\"strokeWidth\"><text text-anchor=\"start\" x=\"{8}\" y=\"{5}\" font-family=\"{4}\" font-size=\"{5}\" fill=\"{6}\">{7}</text></marker></defs>", id, width, fontSize + padding, width / 2, fontFamily, fontSize, color, text, padding);
        }
        #endregion

        protected static string GetMark(Point position, Point direction, string id, double width = 1)
        {
            if (direction.X == 0) direction = direction + new Point(0.001, 0);
            if (direction.Y == 0) direction = direction + new Point(0, 0.001); // for IE bug...
            return string.Format("<path d=\"M {0} {1} l {2} {3}\" fill=\"none\" stroke=\"none\" stroke-width=\"{4}\" marker-start=\"url(#{5})\"/>", position.X, position.Y, direction.X, direction.Y, width, id);
        }        
    }
}
