using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Path : StyledXElement
    {
        public Path(string d, string color, params object[] content) :
            base(SVG.Namespace + "path", null,
                new XAttribute("d", string.Format("{0}", d)),
                new XAttribute("fill", "none"),
                new XAttribute("stroke", color),
                content)
        { }

        public Path(CSS css, string d, params object[] content) :
            base(SVG.Namespace + "path", css,
                new XAttribute("d", string.Format("{0}", d)),
                content)
        { }
    }
}
