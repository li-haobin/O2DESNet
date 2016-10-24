using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class StyledXElement : XElement
    {
        public CSS CSS { get; private set; }
        public StyledXElement(XName name, CSS css, params object[] content) : base(name, content)
        {
            CSS = css;
            if (CSS != null) Add(new XAttribute("class", CSS.Name));
        }
    }

    public class CSS : XElement
    {
        public CSS(string name, params XAttribute[] attributes) : base(name, attributes) { }
        public override string ToString()
        {
            return string.Format(".{0}{{{1}}}", Name, string.Join("", Attributes().Select(attr => string.Format("{0}:{1};", attr.Name, attr.Value))));
        }
    }
}
