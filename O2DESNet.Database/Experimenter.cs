using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace O2DESNet.Database
{
    public class Experimenter<TState> where TState : State
    {
        public string ProjectName { get; private set; }
        public string VersionNumber { get; private set; }
        public Func<int, Dictionary<string, double>, TState> InputFunc { get; private set; }
        public Func<TState, Dictionary<string, double>> OutputFunc { get; private set; }
        public string Operator { get; private set; }
        public int NThreads { get; private set; }
        public int NThreads_Working { get; private set; }

        public Experimenter(DbContext dbContext, string projectName, string versionNumber,
            Func<int, Dictionary<string, double>, TState> inputFunc,
            Func<TState, Dictionary<string, double>> outputFunc,
            TimeSpan runInterval, TimeSpan warmUpPeriod, TimeSpan runLength,
            string operatr = null
            )
        {
            ProjectName = projectName;
            VersionNumber = versionNumber;
            InputFunc = inputFunc;
            OutputFunc = outputFunc;
            Operator = operatr ?? Environment.MachineName;
            var db = new DbContext();
            var version = db.GetVersion(ProjectName, VersionNumber);
            version.RunInterval = runInterval.TotalDays;
            version.WarmUpPeriod = warmUpPeriod.TotalDays;
            version.RunLength = runLength.TotalDays;
            version.Timestamp = DateTime.Now;
            version.Operator = Operator;
            db.SaveChanges();
        }

        public Dictionary<Scenario, double> GetProgress()
        {
            var db = new DbContext();
            var scenarios = db.Scenarios.Where(s => s.Version.Number == VersionNumber && s.Version.Project.Name == ProjectName);
            return scenarios.ToDictionary(s => s, s => s.GetProgress(db));
        }

        public void SetExperiment(Dictionary<string, double> inputValues, int targetNReps = 1)
        {
            var db = new DbContext();
            var scenario = db.GetScenario(ProjectName, VersionNumber, inputValues);
            scenario.TargetNReps = targetNReps;
            scenario.Timestamp = DateTime.Now;
            scenario.Operator = Operator;
            db.SaveChanges();
        }
        /// <summary>
        /// Run experiment with multiple threads
        /// </summary>
        /// <param name="nThreads"></param>
        public void RunExperiment(int nThreads)
        {
            NThreads = nThreads;
            while (NThreads_Working < NThreads)
            {
                Thread thread = new Thread(new ThreadStart(() => RunExperiment()));
                thread.Start();
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// Single thread
        /// </summary>
        private void RunExperiment()
        {
            int threadIdx = Guid.NewGuid().GetHashCode();
            var db = new DbContext();
            lock (this) NThreads_Working++;
            while (NThreads_Working <= NThreads)
            {
                //Console.WriteLine("Thread {0} is listening, total {1}/{2} working threads...",
                //    threadIdx % 100, NThreads_Working, NThreads);                
                var scenario = db.Scenarios.Where(s => s.Replications.Count(r => !r.Excluded) < s.TargetNReps).FirstOrDefault();
                if (scenario != null)
                {
                    #region Create Replication
                    int seed = 0;
                    Replication rep;
                    int thread_uid = Guid.NewGuid().GetHashCode();
                    //using (var transaction = db.Database.BeginTransaction())
                    //{
                    lock (this)
                    {
                        db.Entry(scenario).Collection(s => s.Replications).Query().Load();
                        while (scenario.Replications.Count(r => r.Seed == seed) > 0) seed++;
                        rep = new Replication
                        {
                            Scenario = scenario,
                            Seed = seed,
                            Thread_UID = thread_uid,
                            Timestamp = DateTime.Now,
                            Operator = Operator
                        };
                        scenario.Replications.Add(rep);
                        db.SaveChanges();
                    }
                    //    transaction.Commit();
                    //}               
                    #endregion

                    #region Run and Checkin Snapshot
                    bool thread_check = true;
                    db.Entry(scenario).Collection(s => s.InputValues).Query().Include(i => i.InputPara.InputDesc).Load();
                    db.Entry(scenario).Reference(s => s.Version).Query().Load();
                    var state = InputFunc(seed, scenario.InputValues.ToDictionary(i => i.InputPara.InputDesc.Name, i => i.Value));
                    var sim = new Simulator(state);
                    var ver = scenario.Version;
                    Func<bool> addSnapShot = () =>
                    {
                        db.Entry(rep).GetDatabaseValues();
                        if (rep.Thread_UID == thread_uid)
                        {
                            scenario.AddSnapshot(db, rep.Seed, sim.ClockTime, OutputFunc(state), Environment.MachineName);
                            db.SaveChanges();
                            //Console.WriteLine("Checked in Scenatio {0} Replication {1} Snapshot {2}", scenario.Id, rep.Id, sim.ClockTime);
                            return true;
                        }
                        return false;
                    };
                    while (thread_check && sim.ClockTime < DateTime.MinValue.AddDays(ver.WarmUpPeriod))
                    {
                        sim.Run(TimeSpan.FromDays(ver.RunInterval));
                        thread_check = addSnapShot();
                    }
                    if (thread_check) sim.WarmUp(TimeSpan.FromSeconds(0));
                    while (thread_check && sim.ClockTime < DateTime.MinValue.AddDays(ver.WarmUpPeriod + ver.RunLength))
                    {
                        sim.Run(TimeSpan.FromDays(ver.RunInterval));
                        thread_check = addSnapShot();
                    }
                    #endregion
                }
                Thread.Sleep(2000); /// wait for 2 seconds before attempting next run
            }
            lock (this) NThreads_Working--;
        }
    }
}
