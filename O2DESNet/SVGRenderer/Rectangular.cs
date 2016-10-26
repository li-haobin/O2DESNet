using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Rect : XElement
    {
        public Rect(double x, double y, double width, double height, string stroke, string fill,
            params object[] content) :
            base(SVG.Namespace + "rect",
                new XAttribute("x", x), 
                new XAttribute("y", y), 
                new XAttribute("width", width), 
                new XAttribute("height", height),
                new XAttribute("stroke", stroke),
                new XAttribute("fill", fill),
                content)
        { }
    }
}
