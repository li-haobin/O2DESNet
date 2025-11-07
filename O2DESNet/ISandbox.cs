using Microsoft.Extensions.Logging;

using System;
using System.Collections.Immutable;

namespace O2DESNet;

public interface ISandbox : IDisposable
{
    string Id { get; }
    ILogger? Logger { get; }
    int Seed { get; }

    ISandbox? Parent { get; }
    IImmutableList<ISandbox> Children { get; }
    TimeSpan ClockTime { get; }

    void UpdateRandomSeed(int seed);
    bool Run();
    bool Run(int eventCount);
    bool Run(TimeSpan duration);
    bool Run(double speed);
    bool WarmUp(TimeSpan period);
}
