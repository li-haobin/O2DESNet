using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Line : StyledXElement
    {
        public Line(CSS css, double x1, double y1, double x2, double y2, params object[] content) :
            base(SVG.Namespace + "path", css,
                new XAttribute("d", string.Format("M {0} {1} L {2} {3}", x1, y1, x2, y2)),
                new XAttribute("fill", "none"),
                content)
        { }
    }
}
