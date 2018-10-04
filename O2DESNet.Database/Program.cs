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
            //var db = new DbContext();
            //db.Replications.Find(12).Excluded = true;
            //db.SaveChanges();
            
            var expr = new Experimenter<GGnQueue>(
                dbContext: new DbContext(),
                inputKeys: new string[] { HourlyArrivalRate, HourlyServiceRate, ServerCapacity },
                outputKeys: new string[] { AverageQueueLength, ServerUtilization, NumberOfProcessed },
                projectName: "GGnQ_Experiment", versionNumber: "1.0.0.0",
                inputFunc: InputFunc, outputFunc: OutputFunc,
                runInterval: TimeSpan.FromHours(1),
                warmUpPeriod: TimeSpan.FromHours(2), 
                runLength: TimeSpan.FromDays(1),
                operatr: "Haobin"
                );

            expr.SetExperiment(new Dictionary<string, double> {
                { HourlyArrivalRate, 3 },
                { HourlyServiceRate, 4 },
                { ServerCapacity, 2 },
            }, 2);

            expr.SetExperiment(new Dictionary<string, double> {
                { HourlyArrivalRate, 3 },
                { HourlyServiceRate, 4 },
                { ServerCapacity, 2 },
            }, 30);

            expr.SetExperiment(new Dictionary<string, double> {
                { HourlyArrivalRate, 4 },
                { HourlyServiceRate, 4 },
                { ServerCapacity, 2 },
            }, 30);
            
            expr.ResultsToCSV();

            expr.RunExperiment(10);
            
            while (true)
            {
                var progress = expr.GetProgress();
                Console.Clear();
                foreach (var i in progress)
                {
                    Console.WriteLine("{0}\t{1:F4}", i.Key.Id, i.Value);
                }
                Thread.Sleep(1000);
            }

            Console.ReadKey();
            //while (expr.RunExperiment(0)) ;


            //while (true)
            //{
            //    var db = new DbContext();

            //    var s = db.GetScenario("TuasFinger3", "1.0.0.3",
            //        new Dictionary<string, double> { { "b", 1 }, { "c", 4 } });

            //    s.AddSnapshot(db, 2, new DateTime(2, 1, 1, 0, 1, 0), new Dictionary<string, double> { { "f", 0.01 }, { "g", 400 } }, Environment.MachineName);
            //    //var res = s.RemoveReplication(db, 2);


            //    db.SaveChanges();         
            //}

        }
    }
}
