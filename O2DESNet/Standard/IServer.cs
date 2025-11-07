using System;
using System.Collections.Generic;

namespace O2DESNet.Standard;

public interface IServer : ISandbox
{
    double Capacity { get; }
    int Occupancy { get; }
    double Vacancy { get; }
    double AvgNServing { get; }
    double AvgNOccupying { get; }
    /// <summary>
    /// Utilization only consider serving loads (active)
    /// </summary>
    double UtilServing { get; }
    /// <summary>
    /// Utilization including both serving and served loads (active + passive)
    /// </summary>
    double UtilOccupying { get; }
    IReadOnlyList<IEntity> PendingToStart { get; }
    IReadOnlyList<IEntity> Serving { get; }
    IReadOnlyList<IEntity> PendingToDepart { get; }

    void RqstStart(IEntity load);
    void Depart(IEntity load);

    event Action<IEntity> OnStarted;
    event Action<IEntity> OnReadyToDepart;
}
