namespace O2DESNet.Demos;

public interface IMMnQueue
{
    /// <summary>
    /// Expected number to arrive per hour
    /// </summary>
    double HourlyArrivalRate { get; }
    /// <summary>
    /// Average number to serve per hour by one server
    /// </summary>
    double HourlyServiceRate { get; }
    /// <summary>
    /// Number of servers
    /// </summary>
    int NServers { get; }

    /// <summary>
    /// Average number of loads queueing
    /// </summary>
    double AvgNQueueing { get; }
    /// <summary>
    /// Average number of loads serving
    /// </summary>
    double AvgNServing { get; }
    /// <summary>
    /// Average hours a load spends in system (cycle time)
    /// </summary>
    double AvgHoursInSystem { get; }
}
