using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class AnimateMotion : XElement
    {
        public AnimateMotion(string pathId, params object[] content) : base(
            SVG.Namespace + "animateMotion",
            new XElement(SVG.Namespace + "mpath", new XAttribute("href", "#" + pathId)),
            content)
        { }
    }
}
