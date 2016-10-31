using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class AnimateTranslate : XElement
    {
        public AnimateTranslate(IEnumerable<double> keyTimes, IEnumerable<Tuple<double, double>> xys) :
            base(SVG.Namespace + "animateTransform",
                new XAttribute("attributeName", "transform"),
                new XAttribute("type", "translate"),
                new XAttribute("calcMode", "discrete"),
                new XAttribute("fill", "freeze"),
                new XAttribute("begin", string.Format("{0}s", keyTimes.First())),
                new XAttribute("dur", string.Format("{0}s", keyTimes.Last() - keyTimes.First())),
                new XAttribute("keyTimes", GetKeyTimes(keyTimes)),
                new XAttribute("values", GetValues(xys)))
        {
        }

        private static string GetKeyTimes(IEnumerable<double> keyTimes)
        {
            if (keyTimes.First() != 0) throw new Exception();
            string str = "";
            foreach (var t in keyTimes) str += string.Format("{0};", (t - keyTimes.First()) / (keyTimes.Last() - keyTimes.First()));
            return str.Substring(0, str.Length - 1);
        }

        private static string GetValues(IEnumerable<Tuple<double, double>> xys)
        {
            string str = "";
            foreach (var v in xys) str += string.Format("{0} {1};", v.Item1, v.Item2);
            return str.Substring(0, str.Length - 1);
        }
    }
}
