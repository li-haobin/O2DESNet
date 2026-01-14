using Microsoft.Extensions.Logging;

using O2DESNet.Distributions;
using O2DESNet.HourCounters;
using O2DESNet.Standard;

using System;

namespace O2DESNet.Demos;

public class MMnQueue_Modular : Sandbox, IMMnQueue
{
    #region Static Properties
    public double HourlyArrivalRate { get; private set; }
    public double HourlyServiceRate { get; private set; }
    public int NServers => (int)Server.Capacity;
    #endregion

    #region Dynamic Properties
    public double AvgNQueueing => Queue.AvgNQueueing;
    public double AvgNServing => Server.AvgNServing;
    public double AvgHoursInSystem => HC_InSystem.AverageDuration.TotalHours;

    private IGenerator Generator { get; set; }
    private IQueue Queue { get; set; }
    private IServer Server { get; set; }
    private HourCounter HC_InSystem { get; set; }
    #endregion

    #region Events / Methods
    private void Arrive()
    {
        Logger?.LogInformation("Arrive");
        HC_InSystem.ObserveChange(1, ClockTime);
    }

    private void Depart()
    {
        Logger?.LogInformation("Depart");
        HC_InSystem.ObserveChange(-1, ClockTime);
    }
    #endregion

    public MMnQueue_Modular(double hourlyArrivalRate, double hourlyServiceRate, int nServers, int seed)
        : this(logger: null, hourlyArrivalRate, hourlyServiceRate, nServers, seed) { }

    public MMnQueue_Modular(ILogger? logger, double hourlyArrivalRate, double hourlyServiceRate, int nServers, int seed)
        : base(logger: logger, id: nameof(MMnQueue_Modular), seed)
    {
        HourlyArrivalRate = hourlyArrivalRate;
        HourlyServiceRate = hourlyServiceRate;

        Generator = AddChild(new Generator(logger: logger, new Generator.Statics
        {
            InterArrivalTime = rs => Exponential.Sample(rs, TimeSpan.FromHours(1 / HourlyArrivalRate))
        }, id: nameof(Generator), DefaultRS.Next()));

        Queue = AddChild(new Queue(logger: logger, double.PositiveInfinity, id: nameof(Queue), DefaultRS.Next()));

        Server = AddChild(new Server(logger: logger, new Server.Statics
        {
            Capacity = nServers,
            ServiceTime = (rs, load) => Exponential.Sample(rs, TimeSpan.FromHours(1 / HourlyServiceRate)),
        }, id: nameof(Server), DefaultRS.Next()));

        Generator.OnArrive += () => Queue.RqstEnqueue(new LoadEntity(EntityId.New()));
        Generator.OnArrive += Arrive;

        Queue.OnEnqueued += Server.RqstStart;
        Server.OnStarted += Queue.Dequeue;

        Server.OnReadyToDepart += Server.Depart;
        Server.OnReadyToDepart += load => Depart();

        HC_InSystem = AddHourCounter();

        /// Initial event
        Generator.Start();
    }

    public override void Dispose()
    {
        base.Dispose();
        // No extra unmanaged resources; base disposes children and hour counters.
    }
}
