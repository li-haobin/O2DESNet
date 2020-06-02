using O2DESNet;
using O2DESNet.Distributions;
using O2DESNet.Standard;

using System;

namespace O2DESNet.Demos
{
    public class TandemQueue : Sandbox
    {
        #region Static Properties
        public double HourlyArrivalRate { get; }
        public double HourlyServiceRate1 { get; }
        public double HourlyServiceRate2 { get; }
        public int BufferQueueSize => (int)_queue2.Capacity;

        #endregion

        #region Dynamic Properties
        public double AvgNQueueing1 => _queue1.AvgNQueueing;
        public double AvgNQueueing2 => _queue2.AvgNQueueing;
        public double AvgNServing1 => _server1.AvgNServing;
        public double AvgNServing2 => _server2.AvgNServing;
        public double AvgHoursInSystem => _hcInSystem.AverageDuration.TotalHours;

        private readonly IGenerator _generator;
        private readonly IQueue _queue1;
        private readonly IServer _server1;
        private readonly IQueue _queue2;
        private readonly IServer _server2;
        private readonly HourCounter _hcInSystem;
        #endregion

        #region Events / Methods
        private void Arrive()
        {
            Log("Arrive");
            _hcInSystem.ObserveChange(1, ClockTime);
        }

        private void Depart()
        {
            Log("Depart");
            _hcInSystem.ObserveChange(-1, ClockTime);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TandemQueue"/> class.
        /// </summary>
        /// <param name="arrRate">Hour arrival rate to the system</param>
        /// <param name="svcRate1">Hourly service rate of Server 1</param>
        /// <param name="svcRate2">Hourly service rate of Server 2</param>
        /// <param name="bufferQSize">Buffer queue (Queue 2) capacity</param>
        /// <param name="seed">The seed.</param>
        public TandemQueue(double arrRate, double svcRate1, double svcRate2, int bufferQSize, int seed = 0)
            : base(seed: seed, id: null, pointer: Pointer.Empty)
        {
            HourlyArrivalRate = arrRate;
            HourlyServiceRate1 = svcRate1;
            HourlyServiceRate2 = svcRate2;

            _generator = AddChild(new Generator(new Generator.Statics
            {
                InterArrivalTime = rs => Exponential.Sample(rs, TimeSpan.FromHours(1 / HourlyArrivalRate))
            }, DefaultRS.Next()));

            _queue1 = AddChild(new Queue(double.PositiveInfinity, DefaultRS.Next(), id: "Queue1"));

            _server1 = AddChild(new Server(new Server.Statics
            {
                Capacity = 1,
                ServiceTime = (rs, load) => Exponential.Sample(rs, TimeSpan.FromHours(1 / HourlyServiceRate1)),
            }, DefaultRS.Next(), id: "Server1"));

            _queue2 = AddChild(new Queue(bufferQSize, DefaultRS.Next(), id: "Queue2"));

            _server2 = AddChild(new Server(new Server.Statics
            {
                Capacity = 1,
                ServiceTime = (rs, load) => Exponential.Sample(rs, TimeSpan.FromHours(1 / HourlyServiceRate2)),
            }, DefaultRS.Next(), id: "Server2"));

            _generator.OnArrive += () => _queue1.RequestEnqueue(new Load());
            _generator.OnArrive += Arrive;

            _queue1.OnEnqueued += _server1.RequestStart;
            _server1.OnStarted += _queue1.Dequeue;

            _server1.OnReadyToDepart += _queue2.RequestEnqueue;
            _queue2.OnEnqueued += _server1.Depart;

            _queue2.OnEnqueued += _server2.RequestStart;
            _server2.OnStarted += _queue2.Dequeue;

            _server2.OnReadyToDepart += _server2.Depart;
            _server2.OnReadyToDepart += load => Depart();

            _hcInSystem = AddHourCounter();

            // Initial event
            _generator.Start();
        }

        public override void Dispose() { }
    }
}
