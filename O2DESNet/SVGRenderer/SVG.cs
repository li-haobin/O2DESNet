using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class SVG : XElement
    {
        public static XNamespace Namespace = XNamespace.Get("http://www.w3.org/2000/svg");
        public double Width { get; private set; }
        public double Height { get; private set; }
        public XElement Defs { get; private set; }

        public SVG(double width, double height, params object[] content) : base(Namespace + "svg")
        {
            Width = width; Add(new XAttribute("width", Width));
            Height = height; Add(new XAttribute("height", height));
            Defs = new Definition(); Add(Defs);
            foreach (var cnt in content)
            {
                if (cnt is Definition) Defs.Add(((Definition)cnt).Elements());
                else Add(cnt);
            }
        }

        public void View()
        {
            string file = "view.svg";
            Save(file);
            System.Diagnostics.Process.Start(file);
        }
    }
}
