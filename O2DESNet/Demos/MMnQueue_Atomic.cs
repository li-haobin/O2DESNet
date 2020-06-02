using O2DESNet;
using O2DESNet.Distributions;
using System;

namespace O2DESNet.Demos
{
    public class MMnQueueAtomic : Sandbox, IMMnQueue
    {
        #region Static Properties
        public double HourlyArrivalRate { get; }
        public double HourlyServiceRate { get; }
        public int NServers { get; }
        #endregion

        #region Dynamic Properties / Methods
        public double AvgNQueueing => _hcInQueue.AverageCount;
        public double AvgNServing => _hcInServer.AverageCount;
        public double AvgHoursInSystem => _hcInSystem.AverageDuration.TotalHours;

        private readonly HourCounter _hcInServer;
        private readonly HourCounter _hcInQueue;
        private readonly HourCounter _hcInSystem;
        #endregion

        #region Events
        private void Arrive()
        {
            Log("Arrive");
            _hcInSystem.ObserveChange(1, ClockTime);

            if (_hcInServer.LastCount < NServers) Start();
            else
            {
                Log("Enqueue");
                _hcInQueue.ObserveChange(1, ClockTime);
            }
            Schedule(Arrive, Exponential.Sample(DefaultRS, TimeSpan.FromHours(1 / HourlyArrivalRate)));
        }

        private void Start()
        {
            Log("Start");
            _hcInServer.ObserveChange(1, ClockTime);
            Schedule(Depart, Exponential.Sample(DefaultRS, TimeSpan.FromHours(1 / HourlyServiceRate)));
        }

        private void Depart()
        {
            Log("Depart");
            _hcInServer.ObserveChange(-1, ClockTime);
            _hcInSystem.ObserveChange(-1, ClockTime);

            if (_hcInQueue.LastCount > 0)
            {
                Log("Dequeue");
                _hcInQueue.ObserveChange(-1, ClockTime);
                Start();
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MMnQueueAtomic"/> class.
        /// </summary>
        /// <param name="hourlyArrivalRate">The hourly arrival rate.</param>
        /// <param name="hourlyServiceRate">The hourly service rate.</param>
        /// <param name="nServers">The n servers.</param>
        /// <param name="seed">The seed.</param>
        public MMnQueueAtomic(double hourlyArrivalRate, double hourlyServiceRate, int nServers, int seed = 0)
            : base(seed: seed, id: null, pointer: Pointer.Empty)
        {
            HourlyArrivalRate = hourlyArrivalRate;
            HourlyServiceRate = hourlyServiceRate;
            NServers = nServers;

            _hcInServer = AddHourCounter();
            _hcInQueue = AddHourCounter();
            _hcInSystem = AddHourCounter();

            // Initial event
            Arrive();
        }
    }
}
