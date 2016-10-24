using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Use : XElement
    {
        public Use(string href, params object[] content) :
            base(SVG.Namespace + "use", new XAttribute("href", string.Format("#{0}", href)),
                content)
        { }

        public Use(string href, double x, double y, double degree, params object[] content) :
            base(SVG.Namespace + "use", new XAttribute("href", string.Format("#{0}", href)),
                new XAttribute("transform", string.Format("translate({0},{1}) rotate({2})", x, y, degree)),
                content)
        { }
    }
}
