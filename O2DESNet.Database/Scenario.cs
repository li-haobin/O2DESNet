using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class Scenario
    {
        public int Id { get; set; }
        public Version Version { get; set; }
        public string Comment { get; set; }
        public DateTime CreateTime { get; set; }
        public ICollection<InputValue> InputValues { get; set; } = new HashSet<InputValue>();
        public ICollection<Replication> Replications { get; set; } = new HashSet<Replication>();
        public Snapshot AddSnapshot(DbContext db, int seed, DateTime clockTime, Dictionary<string, double> outputs, string by)
        {
            if (db.Loadable(this)) db.Entry(this).Collection(s => s.Replications).Query().Load();

            #region Get and update replication
            var rep = Replications.Where(r => r.Seed == seed).FirstOrDefault();
            if (rep == null)
            {
                rep = new Replication { Scenario = this, Seed = seed, CreateTime = DateTime.Now, CreateBy = by };
                Replications.Add(rep);
            }
            #endregion

            //if (db.Loadable(rep)) db.Entry(rep).Collection(r => r.Snapshots).Query().Load();
            var snapshot = new Snapshot { Replication = rep, CheckinTime = DateTime.Now, CheckinBy = by, ClockTime = (clockTime - DateTime.MinValue).TotalDays };
            rep.Snapshots.Add(snapshot);
            
            if (db.Loadable(Version))
                db.Entry(Version).Collection(v => v.OutputParas).Query().Include(p => p.OutputDesc).Load();            
            foreach (var i in outputs)
            {
                var para = Version.OutputParas.Where(p => p.OutputDesc.Name == i.Key).FirstOrDefault();
                if (para == null)
                {
                    para = new OutputPara { Version = Version, OutputDesc = Version.Project.GetOutputDesc(db, i.Key) };
                    Version.OutputParas.Add(para);
                }
                snapshot.OutputValues.Add(new OutputValue { OutputPara = para, Value = i.Value, Snapshot = snapshot });
            }
            return snapshot;
        }
        public bool RemoveReplication(int seed)
        {
            throw new NotImplementedException();
        }


    }
}