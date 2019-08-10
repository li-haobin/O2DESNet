using System;
using System.Collections.Generic;

namespace O2DESNet.Standard
{
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
        IReadOnlyList<ILoad> PendingToStart { get; }
        IReadOnlyList<ILoad> Serving { get; }
        IReadOnlyList<ILoad> PendingToDepart { get; }        

        void RqstStart(ILoad load);
        void Depart(ILoad load);

        event Action<ILoad> OnStarted;
        event Action<ILoad> OnReadyToDepart;
    }
}
