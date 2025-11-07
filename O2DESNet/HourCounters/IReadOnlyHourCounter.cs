using Microsoft.Extensions.Logging;
using System;

namespace O2DESNet.HourCounters;

public interface IReadOnlyHourCounter
{
    TimeSpan LastTime { get; }
    double LastCount { get; }
    bool Paused { get; }
    /// <summary>
    /// Total number of increment observed
    /// </summary>
    double TotalIncrement { get; }
    /// <summary>
    /// Total number of decrement observed
    /// </summary>
    double TotalDecrement { get; }
    double IncrementRate { get; }
    double DecrementRate { get; }
    /// <summary>
    /// Total number of hours since the initial time.
    /// </summary>
    double TotalHours { get; }
    double WorkingTimeRatio { get; }
    /// <summary>
    /// The cumulative count value on time in unit of hours
    /// </summary>
    double CumValue { get; }
    /// <summary>
    /// The average count on observation period
    /// </summary>
    double AverageCount { get; }
    /// <summary>
    /// Average timespan that a load stays in the activity, if it is a stationary process, 
    /// i.e., decrement rate == increment rate
    /// It is 0 at the initial status, i.e., decrement rate is NaN (no decrement observed).
    /// </summary>
    TimeSpan AverageDuration { get; }
    ILogger? Logger { get; }
}
