using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet
{
    public class PhaseTracer
    {
        private DateTime _initialTime;
        private int _lastPhaseIndex;
        private Dictionary<string, int> _indices = new Dictionary<string, int>();
        private int GetPhaseIndex(string phase)
        {
            if (!_indices.ContainsKey(phase))
            {
                _indices.Add(phase, AllPhases.Count);
                AllPhases.Add(phase);
                TimeSpans.Add(new TimeSpan());
            }
            return _indices[phase];
        }

        public DateTime LastTime { get; private set; }
        public List<string> AllPhases { get; private set; } = new List<string>();
        public string LastPhase
        {
            get { return AllPhases[_lastPhaseIndex]; }
            private set { _lastPhaseIndex = GetPhaseIndex(value); }
        }

        public List<Tuple<DateTime, int>> History { get; private set; } = new List<Tuple<DateTime, int>>();
        public bool HistoryOn { get; private set; }
        /// <summary>
        /// TimeSpans at all phases
        /// </summary>
        public List<TimeSpan> TimeSpans { get; private set; } = new List<TimeSpan>();
        public PhaseTracer(string initPhase, DateTime? initialTime = null, bool historyOn = false)
        {
            if (initialTime == null) initialTime = DateTime.MinValue;
            _initialTime = initialTime.Value;
            LastTime = _initialTime;
            LastPhase = initPhase;
            HistoryOn = historyOn;
            if (HistoryOn) History = new List<Tuple<DateTime, int>> { new Tuple<DateTime, int>(LastTime, _lastPhaseIndex) };
        }
        public void UpdPhase(string phase, DateTime clockTime)
        {
            var duration = clockTime - LastTime;
            TimeSpans[_lastPhaseIndex] += duration;
            if (HistoryOn) History.Add(new Tuple<DateTime, int>(clockTime, GetPhaseIndex(phase)));
            LastPhase = phase;
            LastTime = clockTime;
        }
        public void WarmedUp(DateTime clockTime)
        {
            _initialTime = clockTime;
            LastTime = clockTime;
            if (HistoryOn) History = new List<Tuple<DateTime, int>> { new Tuple<DateTime, int>(clockTime, _lastPhaseIndex) };
            TimeSpans = TimeSpans.Select(ts => new TimeSpan()).ToList();
        }
        public double GetProportion(string phase, DateTime clockTime)
        {
            if (!_indices.ContainsKey(phase)) return 0;
            double timespan;
            timespan = TimeSpans[_indices[phase]].TotalHours;
            if (phase.Equals(LastPhase)) timespan += (clockTime - LastTime).TotalHours;
            double sum = (clockTime - _initialTime).TotalHours;
            return timespan / sum;
        }
    }
}
