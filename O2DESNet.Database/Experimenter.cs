using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Database
{
    public class Experimenter<TState> where TState : State
    {
        public DbContext _db { get; private set; }
        public string ProjectName { get; private set; }
        public string VersionNumber { get; private set; }
        public Func<int, Dictionary<string, double>, TState> InputFunc { get; private set; }
        public Func<TState, Dictionary<string, double>> OutputFunc { get; private set; }
        public string Operator { get; private set; }

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
            _db = dbContext;
            var version = _db.GetVersion(ProjectName, VersionNumber);
            version.RunInterval = runInterval.TotalDays;
            version.WarmUpPeriod = warmUpPeriod.TotalDays;
            version.RunLength = runLength.TotalDays;
            version.Timestamp = DateTime.Now;
            version.Operator = Operator;
            _db.SaveChanges();
        }

        public void SetNThreads(int nThreads)
        {
            throw new NotImplementedException();
        }

        public void SetExperiment(Dictionary<string, double> inputValues, int targetNReps = 1)
        {
            var scenario = _db.GetScenario(ProjectName, VersionNumber, inputValues);
            scenario.TargetNReps = targetNReps;
            scenario.Timestamp = DateTime.Now;
            scenario.Operator = Operator;
            _db.SaveChanges();
        }

        public bool RunExperiment()
        {
            var scenario = _db.Scenarios.Where(s => s.Replications.Count < s.TargetNReps).FirstOrDefault();
            if (scenario == null) return false;

            _db.Entry(scenario).Collection(s => s.Replications).Query().Load();
            int seed = 0;
            while (scenario.Replications.Count(r => r.Seed == seed) > 0) seed++;
            int thread_uid = Guid.NewGuid().GetHashCode();
            var rep = new Replication
            {
                Scenario = scenario,
                Seed = seed,
                Thread_UID = thread_uid,
                Timestamp = DateTime.Now,
                Operator = Operator
            };
            scenario.Replications.Add(rep);
            _db.SaveChanges();


            /// implement run and checkin snapshot

            return true;
        }
    }
}
