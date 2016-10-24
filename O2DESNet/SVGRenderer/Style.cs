using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Style : XElement
    {
        public Style(params CSS[] cssClasses) : base(SVG.Namespace + "style")
        { Value = string.Join("\n", cssClasses.Select(cls => cls)); }
    }

    
}
