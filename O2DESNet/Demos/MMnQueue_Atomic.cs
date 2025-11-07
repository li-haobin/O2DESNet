using Microsoft.Extensions.Logging;

using O2DESNet.Distributions;
using O2DESNet.HourCounters;

using System;

namespace O2DESNet.Demos;

public class MMnQueue_Atomic : Sandbox, IMMnQueue
{
    #region Static Properties
    public double HourlyArrivalRate { get; private set; }
    public double HourlyServiceRate { get; private set; }
    public int NServers { get; private set; }
    #endregion

    #region Dynamic Properties / Methods
    public double AvgNQueueing => HC_InQueue.AverageCount;
    public double AvgNServing => HC_InServer.AverageCount;
    public double AvgHoursInSystem => HC_InSystem.AverageDuration.TotalHours;

    private HourCounter HC_InServer { get; set; }
    private HourCounter HC_InQueue { get; set; }
    private HourCounter HC_InSystem { get; set; }
    #endregion

    #region Events
    private void Arrive()
    {
        Logger?.LogInformation("Arrive");
        HC_InSystem.ObserveChange(1, ClockTime);

        if (HC_InServer.LastCount < NServers)
            Start();
        else
        {
            Logger?.LogInformation("Enqueue");
            HC_InQueue.ObserveChange(1, ClockTime);
        }

        Schedule(Arrive, Exponential.Sample(DefaultRS, TimeSpan.FromHours(1 / HourlyArrivalRate)));
    }

    private void Start()
    {
        Logger?.LogInformation("Start");
        HC_InServer.ObserveChange(1, ClockTime);
        Schedule(Depart, Exponential.Sample(DefaultRS, TimeSpan.FromHours(1 / HourlyServiceRate)));
    }

    private void Depart()
    {
        Logger?.LogInformation("Depart");
        HC_InServer.ObserveChange(-1, ClockTime);
        HC_InSystem.ObserveChange(-1, ClockTime);

        if (HC_InQueue.LastCount > 0)
        {
            Logger?.LogInformation("Dequeue");
            HC_InQueue.ObserveChange(-1, ClockTime);
            Start();
        }
    }
    #endregion

    public MMnQueue_Atomic(double hourlyArrivalRate, double hourlyServiceRate, int nServers, int seed = 0)
    : base(nameof(MMnQueue_Atomic), seed)
    {
        HourlyArrivalRate = hourlyArrivalRate;
        HourlyServiceRate = hourlyServiceRate;
        NServers = nServers;

        HC_InServer = AddHourCounter();
        HC_InQueue = AddHourCounter();
        HC_InSystem = AddHourCounter();

        /// Initial event
        Arrive();
    }

    public MMnQueue_Atomic(ILogger logger, double hourlyArrivalRate, double hourlyServiceRate, int nServers, int seed = 0)
        : base(logger, nameof(MMnQueue_Atomic), seed)
    {
        HourlyArrivalRate = hourlyArrivalRate;
        HourlyServiceRate = hourlyServiceRate;
        NServers = nServers;

        HC_InServer = AddHourCounter();
        HC_InQueue = AddHourCounter();
        HC_InSystem = AddHourCounter();

        /// Initial event
        Arrive();
    }
}
