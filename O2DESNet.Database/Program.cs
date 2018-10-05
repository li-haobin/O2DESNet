using O2DESNet.Demos.GGnQueue;
using O2DESNet.Distributions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace O2DESNet.Database
{
    class Program
    {
        static string HourlyArrivalRate = "hourly arrival rate";
        static string HourlyServiceRate = "hourly service rate";
        static string ServerCapacity = "server capacity";
        static string AverageQueueLength = "average queue length";
        static string ServerUtilization = "server utilization";
        static string NumberOfProcessed = "number of processed";

        static GGnQueue InputFunc(int seed, Dictionary<string, double> inputValues)
        {
            var config = new GGnQueue.Statics
            {
                InterArrivalTime = rs => TimeSpan.FromHours(Exponential.Sample(rs, 1 / inputValues[HourlyArrivalRate])),
                ServiceTime = (l, rs) => TimeSpan.FromHours(Exponential.Sample(rs, 1 / inputValues[HourlyServiceRate])),
                ServerCapacity = (int)inputValues[ServerCapacity],
            };
            return new GGnQueue(config, seed);
        }
        static Dictionary<string, double> OutputFunc(GGnQueue state)
        {
            return new Dictionary<string, double>
            {
                { AverageQueueLength, state.Queue.HourCounter.AverageCount },
                { ServerUtilization, state.Server.Utilization },
                { NumberOfProcessed, state.Processed.Count }
            };
        }

        static void Main(string[] args)
        {
            var expr = new Experimenter<GGnQueue>(
                dbContext: new DbContext(),
                inputKeys: new string[] { HourlyArrivalRate, HourlyServiceRate, ServerCapacity },
                outputKeys: new string[] { AverageQueueLength, ServerUtilization, NumberOfProcessed },
                projectName: "GGnQ_Experiment", versionNumber: "1.0.0.0",
                inputFunc: InputFunc, outputFunc: OutputFunc,
                runInterval: TimeSpan.FromHours(1),
                warmUpPeriod: TimeSpan.FromHours(2), 
                runLength: TimeSpan.FromDays(1)
                );

            expr.Main();
        }
    }
}
