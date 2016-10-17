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
        public string Source { get { return string.Format("<svg width=\"{0}\" height=\"{1}\" xmlns=\"http://www.w3.org/2000/svg\">\n<defs>\n{2}</defs>\n{3}</svg>", Size.X, Size.Y, Defs, Body); } }
        public void ToFile(string name) { using (var sw = new StreamWriter(name + ".svg")) sw.Write(Source); }
        public void View()
        {
            string name = "view"; ToFile(name);
            Process.Start(string.Format("{0}.svg", name));
        }

        public string Defs { get; set; } = "";
        public string Body { get; set; } = "";

        public SVG() { }

        public SVG(PathMover.Statics pathMover)
        {
            var coords = pathMover.Paths.SelectMany(path => path.Coords).ToList();
            if (coords.Count == 0) return;

            Point min = new Point(coords.Min(pt => pt.X), coords.Min(pt => pt.Y));
            Point max = new Point(coords.Max(pt => pt.X), coords.Max(pt => pt.Y));
            Point margin = new Point(25, 25);
            Size = (max - min) * Scale + margin * 2;
            Reference = margin;

            Defs += GetStyle(
                new Style("path", new Attr("fill", "none"), new Attr("stroke", "black"), new Attr("stroke-width", "1"), new Attr("stroke-dasharray", "3,3")),
                new Style("path_label", new Attr("text-anchor", "start"), new Attr("font-family", "Verdana"), new Attr("font-size", "9px"), new Attr("fill", "black")),
                new Style("cp_label", new Attr("text-anchor", "start"), new Attr("font-family", "Verdana"), new Attr("font-size", "9px"), new Attr("fill", "darkred"))
                );
            Defs += GetArrowMark(id: "arrow", color: "black", width: 10, height: 8);
            Defs += GetCrossMark(id: "cross", color: "darkred", width: 8);            

            var processed = new HashSet<ControlPoint.Statics>();
            foreach (var path in pathMover.Paths)
            {
                coords = path.Coords.Select(pt => (pt - min) * Scale + margin).ToList(); // transform coords for proper view
                // draw the path
                Body += GetPath(path: coords, classId: "path");
                var marks = Point.SlipOnCurve(coords, new List<double> { 1.0 / 3, 2.0 / 3 }.Concat(path.ControlPoints.Select(cp => cp.Positions[path] / path.Length)).ToList());
                // draw the arrow
                if (path.Direction != Path.Direction.Backward) Body += GetUse(id: "arrow", reference: new Point(10, 4), position: marks[0].Item1, direction: marks[0].Item2);
                if (path.Direction != Path.Direction.Forward) Body += GetUse(id: "arrow", reference: new Point(10, 4), position: marks[1].Item1, direction: -marks[1].Item2);
                // draw the mark
                Point position, direction;
                if (path.Direction != Path.Direction.Backward) { position = marks[0].Item1; direction = marks[0].Item2; }
                else { position = marks[1].Item1; direction = marks[1].Item2; }
                Body += GetText(classId: "path_label", text: string.Format("PATH{0}", path.Index), 
                    reference: new Point(-6, -6 - 9), position: position, direction: direction);
                // draw the control points
                for (int i = 2; i < marks.Count; i++)
                    if (!processed.Contains(path.ControlPoints[i - 2]))
                    {
                        Body += GetUse("cross", reference: new Point(4, 4), position: marks[i].Item1, direction: marks[i].Item2);
                        Body += GetText(classId: "cp_label", text: string.Format("CP{0}", path.ControlPoints[i - 2].Index),
                            reference: new Point(-6, 6), position: marks[i].Item1, direction: marks[i].Item2);
                        processed.Add(path.ControlPoints[i - 2]);
                    }
            }
        }

        public class Style
        {
            public string ClassId { get; private set; }
            public Attr[] Attributes { get; private set; }
            public Style(string classId, params Attr[] attributes) { ClassId = classId; Attributes = attributes; }
        }
        public class Attr
        {
            public string Name { get; private set; }
            public string Value { get; private set; }
            public Attr(string name, string value) { Name = name; Value = value; }
        }
        protected static string GetStyle(params Style[] styles)
        {
            string str = "<style type=\"text/css\"><![CDATA[\n";
            foreach (var style in styles)
            {
                str += "." + style.ClassId + "{";
                foreach (var att in style.Attributes) str += string.Format("{0}:{1};", att.Name, att.Value);
                str += "}\n";
            }
            return str + "]]></style>\n";
        }

        protected static string GetArrowMark(string id, string color, double width = 10, double height = 8)
        {
            return string.Format("<path id=\"{0}\" d=\"M 0 0 L {1} {3} L 0 {2}\" fill=\"none\" stroke=\"{4}\" />\n", id, width, height, height / 2, color);
        }

        protected static string GetCrossMark(string id, string color, double width = 8)
        {
            return string.Format("<path id=\"{0}\" d=\"M 0 0 L {1} {1} M 0 {1} L {1} 0\" fill=\"none\" stroke=\"{2}\" />\n", id, width, color);
        }

        protected static string GetUse(string id, Point reference, Point position, Point direction )
        {
            Point translate = position - reference;
            return string.Format("<use href=\"#{0}\" transform=\"translate({1},{2}) rotate({3},{4},{5})\" />\n",
                id, translate.X, translate.Y, direction.Degree(), reference.X, reference.Y);
        }
        
        protected static string GetText(string classId, string text, Point reference, Point position, Point direction)
        {
            Point translate = position - reference;
            return string.Format("<text class=\"{0}\" transform=\"translate({1},{2}) rotate({3}, {4}, {5})\">{6}</text>\n",
                classId, translate.X, translate.Y, direction.Degree(), reference.X, reference.Y, text);
        }

        protected static string GetPath(IEnumerable<Point> path, string classId)
        {
            string str = string.Format("<path class=\"{0}\" d=\"M {1} {2} ", classId, path.First().X, path.First().Y);
            for (int i = 1; i < path.Count(); i++) str += string.Format("L {0} {1}", path.ElementAt(i).X, path.ElementAt(i).Y);
            str += string.Format("\" />\n");
            return str;
        }
    }
}
