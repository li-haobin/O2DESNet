using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;

namespace O2DESNet.Database
{
    public class Scenario
    {
        public int Id { get; set; }
        public Version Version { get; set; }
        public string Comment { get; set; }
        public DateTime Timestamp { get; set; }
        public string Operator { get; set; }
        public ICollection<InputValue> InputValues { get; set; } = new HashSet<InputValue>();
        public ICollection<Replication> Replications { get; set; } = new HashSet<Replication>();        
        public int TargetNReps { get; set; }

        public List<Replication> GetTargetedReps(DbContext db)
        {
            db.Entry(this).Collection(s => s.Replications).Query().Load();
            return Replications.Where(r => r.Scenario.Id == Id && !r.Excluded).OrderBy(r => r.Seed).Take(TargetNReps).ToList();
        }
        public double GetProgress(DbContext db)
        {
            db.Entry(this).Reference(s => s.Version).Query().Load();
            double completedDays = 0;
            foreach (var rep in GetTargetedReps(db))
                if (db.Snapshots.Count(sn => sn.Replication.Id == rep.Id) > 0)
                    completedDays += db.Snapshots.Where(sn => sn.Replication.Id == rep.Id).Max(sn => sn.ClockTime);
            return completedDays / (TargetNReps * (Version.RunLength + Version.WarmUpPeriod));
        }

        public Snapshot AddSnapshot(DbContext db, int seed, DateTime clockTime, Dictionary<string, double> outputs, string by)
        {
            if (db.IsLoadable(this)) db.Entry(this).Collection(s => s.Replications).Query().Load();

            #region Get and update replication
            var rep = Replications.Where(r => r.Seed == seed).FirstOrDefault();
            if (rep == null)
            {
                rep = new Replication { Scenario = this, Seed = seed, Timestamp = DateTime.Now, Operator = by };
                Replications.Add(rep);
            }
            #endregion

            //if (db.Loadable(rep)) db.Entry(rep).Collection(r => r.Snapshots).Query().Load();
            var snapshot = new Snapshot { Replication = rep, CheckinTime = DateTime.Now, CheckinBy = by, ClockTime = (clockTime - DateTime.MinValue).TotalDays };
            rep.Snapshots.Add(snapshot);
            
            if (db.IsLoadable(Version))
                db.Entry(Version).Collection(v => v.OutputParas).Query().Include(p => p.OutputDesc).Load();
            foreach (var i in outputs)
            {
                var para = Version.GetOutputPara(db, i.Key);
                snapshot.OutputValues.Add(new OutputValue { OutputPara = para, Value = i.Value, Snapshot = snapshot });
            }
            return snapshot;
        }
        public bool RemoveReplication(DbContext db, int seed)
        {
            if (db.IsLoadable(this)) db.Entry(this).Collection(s => s.Replications).Query().Load();

            var rep = Replications.Where(r => r.Seed == seed).FirstOrDefault();
            if (rep == null) return false;
            try
            {
                db.Replications.Remove(rep);
            }
            catch { return false; }
            return true;            
        }


    }
}