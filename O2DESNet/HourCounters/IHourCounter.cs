using System;

namespace O2DESNet.HourCounters;

public interface IHourCounter : IReadOnlyHourCounter
{
    void ObserveCount(double count, TimeSpan clockTime);
    void ObserveChange(double count, TimeSpan clockTime);
    void Pause();
    void Pause(TimeSpan clockTime);
    void Resume(TimeSpan clockTime);
}
