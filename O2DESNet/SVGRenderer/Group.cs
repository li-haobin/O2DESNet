using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Group : XElement
    {
        public Group(string id, params object[] content) :
            base(SVG.Namespace + "g", new XAttribute("id", id), content)
        { }

        public Group(string id, double x, double y, double rotate, params object[] content) :
            base(SVG.Namespace + "g", 
                new XAttribute("id", id),
                new XAttribute("transform", string.Format("translate({0} {1}) rotate({2})", x, y, rotate)),
                content)
        { }    
    }
}
