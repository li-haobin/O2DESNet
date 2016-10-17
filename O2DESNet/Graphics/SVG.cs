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
        public string Id { get; set; }
        public Point Size { get; set; } = new Point(100, 100);
        public Point Reference { get; set; } = new Point(150, 150);        
        public string Source { get { return string.Format("<svg width=\"{0}\" height=\"{1}\" xmlns=\"http://www.w3.org/2000/svg\">\n<defs>\n{2}</defs>\n{3}</svg>", Size.X, Size.Y, GetStyle(Styles) + Defs, Body); } }
        public void ToFile(string name) { using (var sw = new StreamWriter(name + ".svg")) sw.Write(Source); }
        public void View()
        {
            string name = "view"; ToFile(name);
            Process.Start(string.Format("{0}.svg", name));
        }

        public List<Style> Styles { get; private set; } = new List<Style>();
        public string Defs { get; set; } = "";
        public string Body { get; set; } = "";
        
        public class Style
        {
            public string ClassId { get; private set; }
            public Attr[] Attributes { get; private set; }
            public Style(string classId, params Attr[] attributes) { ClassId = classId; Attributes = attributes; }
            public static string ToString(Style style, Func<string,string, string> format)
            {
                string str = ""; foreach (var attr in style.Attributes) str += format(attr.Name, attr.Value); return str;
            }
        }
        public class Attr
        {
            public string Name { get; private set; }
            public string Value { get; private set; }
            public Attr(string name, string value) { Name = name; Value = value; }
        }
        private static string GetStyle(List<Style> styles)
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

        public static string GetArrowMark(string id, string color, double width = 10, double height = 8)
        {
            return string.Format("<path id=\"{0}\" d=\"M 0 0 L {1} {3} L 0 {2}\" fill=\"none\" stroke=\"{4}\" />\n", id, width, height, height / 2, color);
        }

        public static string GetCrossMark(string id, string color, double width = 8)
        {
            return string.Format("<path id=\"{0}\" d=\"M 0 0 L {1} {1} M 0 {1} L {1} 0\" fill=\"none\" stroke=\"{2}\" />\n", id, width, color);
        }

        public static string GetUse(string id, Point reference, Tuple<Point, double> posture)
        {
            Point translate = posture.Item1 - reference;
            return string.Format("<use href=\"#{0}\" transform=\"translate({1},{2}) rotate({3},{4},{5})\" />\n",
                id, translate.X, translate.Y, posture.Item2, reference.X, reference.Y);
        }

        public static string GetText(string text, string classId, Point reference, Tuple<Point, double> posture)
        {
            Point translate = posture.Item1 - reference;
            return string.Format("<text class=\"{0}\" transform=\"translate({1},{2}) rotate({3}, {4}, {5})\">{6}</text>\n",
                classId, translate.X, translate.Y, posture.Item2, reference.X, reference.Y, text);
        }

        public static string GetText(string text, Style style, Point reference, Tuple<Point, double> posture)
        {
            Point translate = posture.Item1 - reference;
            return string.Format("<text {0} transform=\"translate({1},{2}) rotate({3}, {4}, {5})\">{6}</text>\n",
                Style.ToString(style, (name, value) => string.Format("{0}=\"{1}\" ", name, value)),
                translate.X, translate.Y, posture.Item2, reference.X, reference.Y, text);
        }

        public static string GetPath(IEnumerable<Point> path, string classId)
        {
            string str = string.Format("<path class=\"{0}\" d=\"M {1} {2} ", classId, path.First().X, path.First().Y);
            for (int i = 1; i < path.Count(); i++) str += string.Format("L {0} {1}", path.ElementAt(i).X, path.ElementAt(i).Y);
            str += string.Format("\" />\n");
            return str;
        }
    }
}
