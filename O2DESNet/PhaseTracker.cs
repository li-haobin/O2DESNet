using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet
{
    public class PhaseTracer
    {
        private DateTime _initialTime;
        private int _lastPhaseIndex;
        private readonly Dictionary<string, int> _indices = [];

        private int GetPhaseIndex(string phase)
        {
            if (!_indices.ContainsKey(phase))
            {
                _indices.Add(phase, AllPhases.Count);
                AllPhases.Add(phase);
                TimeSpans.Add(TimeSpan.Zero);
            }
            return _indices[phase];
        }

        public DateTime LastTime { get; private set; }
        public List<string> AllPhases { get; private set; } = [];
        public string LastPhase
        {
            get => AllPhases[_lastPhaseIndex];
            private set => _lastPhaseIndex = GetPhaseIndex(value);
        }

        public List<Tuple<DateTime, int>> History { get; private set; } = [];
        public bool HistoryOn { get; }

        /// <summary>
        /// TimeSpans at all phases
        /// </summary>
        public List<TimeSpan> TimeSpans { get; private set; } = [];

        public PhaseTracer(string initPhase, DateTime? initialTime = null, bool historyOn = false)
        {
            _initialTime = initialTime ?? DateTime.MinValue;
            LastTime = _initialTime;
            LastPhase = initPhase;
            HistoryOn = historyOn;
            if (HistoryOn) History = [Tuple.Create(LastTime, _lastPhaseIndex)];
        }

        public void UpdPhase(string phase, DateTime clockTime)
        {
            var duration = clockTime - LastTime;
            TimeSpans[_lastPhaseIndex] += duration;
            if (HistoryOn) History.Add(Tuple.Create(clockTime, GetPhaseIndex(phase)));
            LastPhase = phase;
            LastTime = clockTime;
        }

        public void WarmedUp(DateTime clockTime)
        {
            _initialTime = clockTime;
            LastTime = clockTime;
            if (HistoryOn) History = [Tuple.Create(clockTime, _lastPhaseIndex)];
            TimeSpans = TimeSpans.Select(_ => TimeSpan.Zero).ToList();
        }

        public double GetProportion(string phase, DateTime clockTime)
        {
            if (!_indices.ContainsKey(phase)) return 0;
            var timespan = TimeSpans[_indices[phase]].TotalHours;
            if (phase.Equals(LastPhase)) timespan += (clockTime - LastTime).TotalHours;
            var sum = (clockTime - _initialTime).TotalHours;
            return timespan / sum;
        }
    }
}
