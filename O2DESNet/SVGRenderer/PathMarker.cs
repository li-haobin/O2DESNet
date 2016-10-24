using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class PathMarker: Group
    {
        public PathMarker(string id, string pathId, double ratio, params object[] content) : base(id,
            new AnimateMotion(pathId, 
                new XAttribute("fill", "freeze"), 
                new XAttribute("rotate", "auto"), 
                new XAttribute("keyTimes", "0;1"), 
                new XAttribute("keyPoints", string.Format("{0};{0}", ratio)), 
                new XAttribute("calcMode", "linear")),
            content) { }
    }
}
