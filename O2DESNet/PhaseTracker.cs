using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace O2DESNet
{
    public class PhaseTracker<TPhase>
    {
        private DateTime _initialTime;
        public DateTime LastTime;
        public TPhase LastPhase { get; private set; }
        public List<Tuple<DateTime, TPhase>> History { get; private set; }
        public Dictionary<TPhase, TimeSpan> TimeSpans { get; private set; }
        public PhaseTracker(TPhase initPhase)
        {
            LastTime = _initialTime;
            LastPhase = initPhase;
            History = new List<Tuple<DateTime, TPhase>> { new Tuple<DateTime, TPhase>(LastTime, LastPhase) };
            TimeSpans = new Dictionary<TPhase, TimeSpan>();
        }
        public void UpdPhase(TPhase phase, DateTime clockTime)
        {
            History.Add(new Tuple<DateTime, TPhase>(clockTime, phase));
            var duration = clockTime - LastTime;
            if (!TimeSpans.ContainsKey(LastPhase)) TimeSpans.Add(LastPhase, duration);
            else TimeSpans[LastPhase] += duration;
            LastPhase = phase;
            LastTime = clockTime;
        }
        public void WarmedUp(DateTime clockTime)
        {
            _initialTime = clockTime;
            LastTime = clockTime;
            History = new List<Tuple<DateTime, TPhase>> { new Tuple<DateTime, TPhase>(clockTime, LastPhase) };
            TimeSpans = new Dictionary<TPhase, TimeSpan>();
        }
        public double GetProportion(TPhase phase, DateTime clockTime)
        {
            double timespan;
            if (!TimeSpans.ContainsKey(phase)) timespan = 0;
            else timespan = TimeSpans[phase].TotalHours;
            if (phase.Equals(LastPhase)) timespan += (clockTime - LastTime).TotalHours;
            double sum = (clockTime - _initialTime).TotalHours;
            return timespan / sum;
        }
        public Canvas GetDrawing(DateTime clockTime, Dictionary<TPhase, SolidColorBrush> colors, double length = 300, double height = 20)
        {
            var canvas = new Canvas();
            var totalHours = (clockTime - _initialTime).TotalHours;
            for (int i = 0; i < History.Count - 1; i++)
            {
                if (colors.ContainsKey(History[i].Item2))
                {
                    var x0 = (History[i].Item1 - _initialTime).TotalHours / totalHours * length;
                    var x1 = (History[i + 1].Item1 - _initialTime).TotalHours / totalHours * length;
                    canvas.Children.Add(new System.Windows.Shapes.Rectangle
                    {
                        Width = x1 - x0,
                        Height = height,
                        RenderTransform = new TranslateTransform(x0, 0),
                        Fill = colors[History[i].Item2],
                    });
                }
            }
            return canvas;
        }
    }
}
