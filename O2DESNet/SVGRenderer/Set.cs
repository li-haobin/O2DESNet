using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Set:XElement
    {
       public Set(string attributeName, string to, double begin, params object[] content) :
            base(SVG.Namespace + "set",
                new XAttribute("attributeName", attributeName),
                new XAttribute("to", to),
                new XAttribute("begin", begin),
                new XAttribute("fill", "freeze"),
                content)
        {
        }
    }
}
