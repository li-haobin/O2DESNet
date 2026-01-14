using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet;

public class PhaseTracer
{
    private TimeSpan _initialTime;
    private int _lastPhaseIndex;
    private Dictionary<string, int> _indices = [];
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

    public TimeSpan LastTime { get; private set; }
    public List<string> AllPhases { get; private set; } = [];
    public string LastPhase
    {
        get { return AllPhases[_lastPhaseIndex]; }
        private set { _lastPhaseIndex = GetPhaseIndex(value); }
    }

    public List<Tuple<TimeSpan, int>> History { get; private set; } = [];
    public bool HistoryOn { get; private set; }
    /// <summary>
    /// TimeSpans at all phases
    /// </summary>
    public List<TimeSpan> TimeSpans { get; private set; } = [];
    public PhaseTracer(string initPhase, TimeSpan? initialTime = null, bool historyOn = false)
    {
        if (initialTime == null)
            initialTime = TimeSpan.Zero;
        _initialTime = initialTime.Value;
        LastTime = _initialTime;
        LastPhase = initPhase;
        HistoryOn = historyOn;
        if (HistoryOn)
            History = [new Tuple<TimeSpan, int>(LastTime, _lastPhaseIndex)];
    }
    public void UpdPhase(string phase, TimeSpan clockTime)
    {
        var duration = clockTime - LastTime;
        TimeSpans[_lastPhaseIndex] += duration;
        if (HistoryOn)
            History.Add(new Tuple<TimeSpan, int>(clockTime, GetPhaseIndex(phase)));
        LastPhase = phase;
        LastTime = clockTime;
    }
    public void WarmedUp(TimeSpan clockTime)
    {
        _initialTime = clockTime;
        LastTime = clockTime;
        if (HistoryOn)
            History = [new Tuple<TimeSpan, int>(clockTime, _lastPhaseIndex)];
        TimeSpans = TimeSpans.Select(ts => new TimeSpan()).ToList();
    }
    public double GetProportion(string phase, TimeSpan clockTime)
    {
        if (!_indices.ContainsKey(phase))
            return 0;
        double timespan;
        timespan = TimeSpans[_indices[phase]].TotalHours;
        if (phase.Equals(LastPhase))
            timespan += (clockTime - LastTime).TotalHours;
        double sum = (clockTime - _initialTime).TotalHours;
        return sum == 0 ? 0 : timespan / sum;
    }
}
