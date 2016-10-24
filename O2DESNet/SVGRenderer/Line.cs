using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Line : XElement
    {
        public Line(double x1, double y1, double x2, double y2, string stroke, params object[] content) :
            base(SVG.Namespace + "path",
                new XAttribute("d", string.Format("M {0} {1} L {2} {3}", x1, y1, x2, y2)),
                new XAttribute("fill", "none"),
                new XAttribute("stroke", stroke),
                content)
        { }
    }
}
