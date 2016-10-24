using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Text : StyledXElement
    {
        public Text(CSS css, string text, params object[] content) :
            base(SVG.Namespace + "text", css,
                content)
        { Value = text; }
    }
}
