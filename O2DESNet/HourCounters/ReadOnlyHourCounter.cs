using Microsoft.Extensions.Logging;
using System;

namespace O2DESNet.HourCounters;

public class ReadOnlyHourCounter : IReadOnlyHourCounter, IDisposable
{
    public TimeSpan LastTime => HourCounter.LastTime;

    public double LastCount => HourCounter.LastCount;

    public bool Paused => HourCounter.Paused;

    public double TotalIncrement => HourCounter.TotalIncrement;

    public double TotalDecrement => HourCounter.TotalDecrement;

    public double IncrementRate => HourCounter.IncrementRate;

    public double DecrementRate => HourCounter.DecrementRate;

    public double TotalHours => HourCounter.TotalHours;

    public double WorkingTimeRatio => HourCounter.WorkingTimeRatio;

    public double CumValue => HourCounter.CumValue;

    public double AverageCount => HourCounter.AverageCount;

    public TimeSpan AverageDuration => HourCounter.AverageDuration;

    public ILogger? Logger => HourCounter.Logger;

    private readonly HourCounter HourCounter;
    internal ReadOnlyHourCounter(HourCounter hourCounter)
    {
        HourCounter = hourCounter;
    }

    public void Dispose() { }
}
