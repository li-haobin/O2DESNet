using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Definition : XElement
    {
        public Definition(params object[] content) : base(SVG.Namespace + "defs", content)
        { }
    }
}
