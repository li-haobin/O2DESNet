using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Animate : XElement
    {
        public Animate(string attributeName, IEnumerable<double> keyTimes, IEnumerable<object> values,
           params object[] content) :
            base(SVG.Namespace + "animate",
                new XAttribute("attributeName", attributeName),
                new XAttribute("dur", string.Format("{0}s", keyTimes.Last())),
                new XAttribute("keyTimes", GetKeyTimes(keyTimes)),
                new XAttribute("values", GetValues(values)),
                content)
        {
        }

        private static string GetKeyTimes(IEnumerable<double> keyTimes)
        {
            if (keyTimes.First() != 0) throw new Exception();
            string str = "";
            foreach (var t in keyTimes) str += string.Format("{0};", t / keyTimes.Last());
            return str.Substring(0, str.Length - 1);
        }

        private static string GetValues(IEnumerable<object> values)
        {
            string str = "";
            foreach (var v in values) str += string.Format("{0};", v);
            return str.Substring(0, str.Length - 1);
        }
    }
}
