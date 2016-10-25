using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace O2DESNet.SVGRenderer
{
    public class Clock : Group
    {
        private static string _h = "hidden";
        private static string _v = "visible";

        private Group FlashDigit(CSS css, int digit, double dur, double start, double end)
        {
            if (start < end)
                return new Group(new Text(css, digit.ToString()), new Animate("visibility",
                    new List<double> { 0, start, end, dur },
                    new object[] { _h, _v, _h, _h },
                    new XAttribute("repeatCount", "indefinite")));
            else
                return new Group(new Text(css, digit.ToString()), new Animate("visibility",
                    new List<double> { 0, end, start, dur },
                    new object[] { _v, _h, _v, _v },
                    new XAttribute("repeatCount", "indefinite")));
        }

        public Group CycledDigits(CSS css, double pos, List<int> digits, double dur, double init)
        {
            double interval = dur / digits.Count;
            var g = new Group(new XAttribute("transform", string.Format("translate({0} 0)", pos)));
            for (int i = 0; i < digits.Count; i++)
            {
                var start = interval * i - init; while (start < 0) start += dur; while (start > dur) start -= dur;
                var end = interval * (i + 1) - init; while (end < 0) end += dur; while (end > dur) end -= dur;
                g.Add(FlashDigit(css, digits[i], dur, start, end));
            }
            return g;
        }

        public Clock(DateTime startTime, double speed = 1) : base()
        {
            string color = "black";
            int size = 20, digitWidth = 10;
            var css = new CSS("digital_clock", new XAttribute("text-anchor", "middle"), new XAttribute("font-family", "Calibri"), new XAttribute("font-size", size + "px"), new XAttribute("fill", color));

            Add(new XAttribute("transform", string.Format("translate(0 {0})", size)));
            Add(new Style(css));
            Add(CycledDigits(css, digitWidth * 0.5, new List<int> { 0, 1, 2 }, 3600 * 24 / speed, startTime.Hour * 3600 / speed)); // hour x 10
            Add(CycledDigits(css, digitWidth * 1.5, new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3600 * 10 / speed, startTime.Hour % 10 * 3600 / speed)); // hour x 1
            Add(new Text(css, ":", new XAttribute("transform", string.Format("translate({0} 0)", digitWidth * 2.5))));
            Add(CycledDigits(css, digitWidth * 3.5, new List<int> { 0, 1, 2, 3, 4, 5 }, 3600 / speed, startTime.Minute * 60 / speed)); // minute x 10
            Add(CycledDigits(css, digitWidth * 4.5, new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 600 / speed, startTime.Minute % 10 * 60 / speed)); // minute x 1
            Add(new Text(css, ":", new XAttribute("transform", string.Format("translate({0} 0)", digitWidth * 5.5))));
            Add(CycledDigits(css, digitWidth * 6.5, new List<int> { 0, 1, 2, 3, 4, 5 }, 60 / speed, startTime.Second / speed)); // second x 10
            Add(CycledDigits(css, digitWidth * 7.5, new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 10 / speed, startTime.Second % 10 / speed)); // second x 1
        }
    }

}
