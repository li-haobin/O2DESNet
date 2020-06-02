using O2DESNet;
using O2DESNet.Distributions;
using O2DESNet.Standard;
using System;

namespace O2DESNet.Demos
{
    public class MMnQueue_Modular : Sandbox, IMMnQueue
    {
        #region Static Properties
        public double HourlyArrivalRate { get; }
        public double HourlyServiceRate { get; }
        public int NServers => (int)Server.Capacity;

        #endregion

        #region Dynamic Properties
        public double AvgNQueueing => Queue.AvgNQueueing;
        public double AvgNServing => Server.AvgNServing;
        public double AvgHoursInSystem => HC_InSystem.AverageDuration.TotalHours;

        private IGenerator Generator { get; }
        private IQueue Queue { get; }
        private IServer Server { get; }
        private HourCounter HC_InSystem { get; }
        #endregion

        #region Events / Methods
        private void Arrive()
        {
            Log("Arrive");
            HC_InSystem.ObserveChange(1, ClockTime);
        }

        private void Depart()
        {
            Log("Depart");
            HC_InSystem.ObserveChange(-1, ClockTime);
        }
        #endregion

        public MMnQueue_Modular(double hourlyArrivalRate, double hourlyServiceRate, int nServers, int seed = 0)
            : base(seed)
        {
            HourlyArrivalRate = hourlyArrivalRate;
            HourlyServiceRate = hourlyServiceRate;

            Generator = AddChild(new Generator(new Generator.Statics
            {
                InterArrivalTime = rs => Exponential.Sample(rs, TimeSpan.FromHours(1 / HourlyArrivalRate))
            }, DefaultRS.Next()));

            Queue = AddChild(new Queue(double.PositiveInfinity, DefaultRS.Next()));

            Server = AddChild(new Server(new Server.Statics
            {
                Capacity = nServers,
                ServiceTime = (rs, load) => Exponential.Sample(rs, TimeSpan.FromHours(1 / HourlyServiceRate)),
            }, DefaultRS.Next()));

            Generator.OnArrive += () => Queue.RqstEnqueue(new Load());
            Generator.OnArrive += Arrive;

            Queue.OnEnqueued += Server.RqstStart;
            Server.OnStarted += Queue.Dequeue;

            Server.OnReadyToDepart += Server.Depart;
            Server.OnReadyToDepart += load => Depart();

            HC_InSystem = AddHourCounter();

            /// Initial event
            Generator.Start();
        }

        public override void Dispose() { }
    }
}
