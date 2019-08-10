using O2DESNet;
using O2DESNet.Distributions;
using System;

namespace O2DESNet.Demos
{
    public class MMnQueue_Atomic : Sandbox, IMMnQueue
    {
        #region Static Properties
        public double HourlyArrivalRate { get; private set; }
        public double HourlyServiceRate { get; private set; }
        public int NServers { get; private set; }
        #endregion

        #region Dynamic Properties / Methods
        public double AvgNQueueing { get { return HC_InQueue.AverageCount; } }
        public double AvgNServing { get { return HC_InServer.AverageCount; } }
        public double AvgHoursInSystem { get { return HC_InSystem.AverageDuration.TotalHours; } }

        private HourCounter HC_InServer { get; set; }
        private HourCounter HC_InQueue { get; set; }
        private HourCounter HC_InSystem { get; set; }
        #endregion

        #region Events
        private void Arrive()
        {
            Log("Arrive");
            HC_InSystem.ObserveChange(1, ClockTime);

            if (HC_InServer.LastCount < NServers) Start();
            else
            {
                Log("Enqueue");
                HC_InQueue.ObserveChange(1, ClockTime);
            }
            Schedule(Arrive, Exponential.Sample(DefaultRS, TimeSpan.FromHours(1 / HourlyArrivalRate)));
        }

        private void Start()
        {
            Log("Start");
            HC_InServer.ObserveChange(1, ClockTime);
            Schedule(Depart, Exponential.Sample(DefaultRS, TimeSpan.FromHours(1 / HourlyServiceRate)));
        }

        private void Depart()
        {
            Log("Depart");
            HC_InServer.ObserveChange(-1, ClockTime);
            HC_InSystem.ObserveChange(-1, ClockTime);

            if (HC_InQueue.LastCount > 0)
            {
                Log("Dequeue");
                HC_InQueue.ObserveChange(-1, ClockTime);
                Start();
            }
        }
        #endregion

        public MMnQueue_Atomic(double hourlyArrivalRate, double hourlyServiceRate, int nServers, int seed = 0)
            : base(seed)
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
}
