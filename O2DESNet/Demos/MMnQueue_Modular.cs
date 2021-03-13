using O2DESNet.Distributions;
using O2DESNet.Standard;

using System;

namespace O2DESNet.Demos
{
    public class MMnQueueModular : Sandbox, IMMnQueue
    {
        #region Static Properties
        public double HourlyArrivalRate { get; }
        public double HourlyServiceRate { get; }
        public int NServers => (int)_server.Capacity;

        #endregion

        #region Dynamic Properties
        public double AvgNQueueing => _queue.AvgNQueueing;
        public double AvgNServing => _server.AvgNServing;
        public double AvgHoursInSystem => _hcInSystem.AverageDuration.TotalHours;

        private readonly IQueue _queue;
        private readonly IServer _server;
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
        /// Initializes a new instance of the <see cref="MMnQueueModular"/> class.
        /// </summary>
        /// <param name="hourlyArrivalRate">The hourly arrival rate.</param>
        /// <param name="hourlyServiceRate">The hourly service rate.</param>
        /// <param name="nServers">The n servers.</param>
        /// <param name="seed">The seed.</param>
        public MMnQueueModular(double hourlyArrivalRate, double hourlyServiceRate, int nServers, int seed = 0)
            : base(seed: seed, id: null, pointer: Pointer.Empty)
        {
            HourlyArrivalRate = hourlyArrivalRate;
            HourlyServiceRate = hourlyServiceRate;

            IGenerator generator = AddChild(new Generator(new Generator.Statics
            {
                InterArrivalTime = rs => Exponential.Sample(rs, TimeSpan.FromHours(1 / HourlyArrivalRate))
            }, DefaultRS.Next()));

            _queue = AddChild(new Queue(double.PositiveInfinity, DefaultRS.Next(), id: null) { DebugMode = true });

            _server = AddChild(new Server(new Server.Statics
            {
                Capacity = nServers,
                ServiceTime = (rs, load) => Exponential.Sample(rs, TimeSpan.FromHours(1 / HourlyServiceRate)),
            }, DefaultRS.Next()));

            generator.OnArrive += () => _queue.RequestEnqueue(new Load());
            generator.OnArrive += Arrive;

            _queue.OnEnqueued += _server.RequestStart;
            _server.OnStarted += _queue.Dequeue;

            _server.OnReadyToDepart += _server.Depart;
            _server.OnReadyToDepart += load => Depart();

            _hcInSystem = AddHourCounter();

            // Initial event
            generator.Start();
        }

        public override void Dispose() { }
    }
}
