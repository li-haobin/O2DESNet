using System;

namespace O2DESNet.Standard;

public interface IGenerator : ISandbox
{
    TimeSpan? StartTime { get; }
    bool IsOn { get; }
    /// <summary>
    /// Number of loads generated
    /// </summary>
    int Count { get; }
    /// <summary>
    /// Input event - Start
    /// </summary>
    void Start();
    /// <summary>
    /// Input event - End
    /// </summary>
    void End();
    /// <summary>
    /// Output event - Arrive
    /// </summary>
    event Action OnArrive;
}
